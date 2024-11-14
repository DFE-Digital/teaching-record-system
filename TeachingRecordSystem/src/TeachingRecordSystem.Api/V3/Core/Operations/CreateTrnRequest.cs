using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record CreateTrnRequestCommand
{
    public required string RequestId { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required IReadOnlyCollection<string> EmailAddresses { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required string? VerifiedOneLoginUserSubject { get; init; }
}

public class CreateTrnRequestHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    TrnRequestHelper trnRequestHelper,
    ICurrentUserProvider currentUserProvider,
    ITrnGenerationApiClient trnGenerationApiClient,
    INameSynonymProvider nameSynonymProvider,
    MessageSerializer messageSerializer)
{
    public async Task<TrnRequestInfo> Handle(CreateTrnRequestCommand command)
    {
        var currentApplicationUserId = currentUserProvider.GetCurrentApplicationUserId();

        var trnRequest = await trnRequestHelper.GetTrnRequestInfo(currentApplicationUserId, command.RequestId);
        if (trnRequest is not null)
        {
            throw new ErrorException(ErrorRegistry.CannotResubmitRequest());
        }

        // Normalize names; DQT matching process requires a single-word first name :-|
        var firstAndMiddleNames = $"{command.FirstName} {command.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = firstAndMiddleNames.First();
        var middleName = string.Join(' ', firstAndMiddleNames.Skip(1));

        var firstNameSynonyms = (await nameSynonymProvider.GetAllNameSynonyms()).GetValueOrDefault(firstName, []);

        var normalizedNino = NationalInsuranceNumberHelper.Normalize(command.NationalInsuranceNumber);

        // Check workforce data for NINO matches
        var workforceDataMatches = normalizedNino is not null ?
            await dbContext.TpsEmployments
                .Where(e => e.NationalInsuranceNumber == normalizedNino)
                .Join(dbContext.Persons, e => e.PersonId, p => p.PersonId, (e, p) => p.DqtContactId!.Value)
                .ToArrayAsync() :
            [];

        var potentialDuplicates = (await crmQueryDispatcher.ExecuteQuery(
            new FindPotentialDuplicateContactsQuery()
            {
                FirstNames = firstNameSynonyms.Append(firstName),
                MiddleName = middleName,
                LastName = command.LastName,
                DateOfBirth = command.DateOfBirth,
                EmailAddresses = command.EmailAddresses,
                NationalInsuranceNumber = normalizedNino,
                MatchedOnNationalInsuranceNumberContactIds = workforceDataMatches
            })).ToList();

        // If any record has matched on NINO & DOB treat that as a definite match and return the existing record's details
        var matchedOnNinoAndDob = potentialDuplicates
            .FirstOrDefault(d => d.MatchedAttributes.Contains(Contact.Fields.dfeta_NINumber) && d.MatchedAttributes.Contains(Contact.Fields.BirthDate));
        if (matchedOnNinoAndDob is not null)
        {
            // FUTURE: consider whether we should be updating any missing attributes here

            var hasStatedNames = !string.IsNullOrEmpty(matchedOnNinoAndDob.StatedFirstName) &&
                !string.IsNullOrEmpty(matchedOnNinoAndDob.StatedLastName);

            return new TrnRequestInfo()
            {
                RequestId = command.RequestId,
                Person = new TrnRequestInfoPerson()
                {
                    FirstName = hasStatedNames ? matchedOnNinoAndDob.StatedFirstName! : matchedOnNinoAndDob.FirstName,
                    MiddleName = hasStatedNames ? matchedOnNinoAndDob.StatedMiddleName ?? "" : matchedOnNinoAndDob.MiddleName,
                    LastName = hasStatedNames ? matchedOnNinoAndDob.StatedLastName! : matchedOnNinoAndDob.LastName,
                    EmailAddress = matchedOnNinoAndDob.EmailAddress,
                    DateOfBirth = matchedOnNinoAndDob.DateOfBirth!.Value,
                    NationalInsuranceNumber = NationalInsuranceNumberHelper.Normalize(matchedOnNinoAndDob.NationalInsuranceNumber)
                },
                Trn = matchedOnNinoAndDob.Trn,
                Status = TrnRequestStatus.Completed
            };
        }

        string? trn = null;
        if (potentialDuplicates.Count == 0)
        {
            trn = await trnGenerationApiClient.GenerateTrn();
        }

        var potentialDuplicatePersonIds = potentialDuplicates.Select(d => d.ContactId).ToList();
        var resultsWithActiveAlerts = await dbContext.Alerts
            .Where(a => potentialDuplicatePersonIds.Contains(a.PersonId) && a.IsOpen)
            .Select(a => a.PersonId)
            .Distinct()
            .ToArrayAsync();

        var emailAddress = command.EmailAddresses?.FirstOrDefault();

        var outboxMessages = new List<dfeta_TrsOutboxMessage>();
        if (command.VerifiedOneLoginUserSubject is string oneLoginUserId)
        {
            outboxMessages.Add(messageSerializer.CreateCrmOutboxMessage(new TrnRequestMetadataMessage()
            {
                ApplicationUserId = currentApplicationUserId,
                RequestId = command.RequestId,
                VerifiedOneLoginUserSubject = oneLoginUserId
            }));
        }

        await crmQueryDispatcher.ExecuteQuery(new CreateContactQuery()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = command.LastName,
            StatedFirstName = command.FirstName,
            StatedMiddleName = command.MiddleName ?? "",
            StatedLastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = NationalInsuranceNumberHelper.Normalize(command.NationalInsuranceNumber),
            PotentialDuplicates = potentialDuplicates.Select(d => (Duplicate: d, HasActiveAlert: resultsWithActiveAlerts.Contains(d.ContactId))).ToArray(),
            Trn = trn,
            TrnRequestId = TrnRequestHelper.GetCrmTrnRequestId(currentApplicationUserId, command.RequestId),
            OutboxMessages = outboxMessages
        });

        var status = trn is not null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
            Person = new TrnRequestInfoPerson()
            {
                FirstName = command.FirstName,
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                EmailAddress = emailAddress,
                DateOfBirth = command.DateOfBirth,
                NationalInsuranceNumber = command.NationalInsuranceNumber
            },
            Trn = trn,
            Status = status
        };
    }
}
