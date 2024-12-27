using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using TeachingRecordSystem.Core.Services.NameSynonyms;
using TeachingRecordSystem.Core.Services.TrnGenerationApi;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record CreateTrnRequestCommand
{
    public required string RequestId { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required IReadOnlyCollection<string> EmailAddresses { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required bool? IdentityVerified { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required Gender? Gender { get; init; }
    public required string? AddressLine1 { get; init; }
    public required string? AddressLine2 { get; init; }
    public required string? AddressLine3 { get; init; }
    public required string? City { get; init; }
    public required string? Postcode { get; init; }
    public required string? Country { get; init; }
}

public class CreateTrnRequestHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    TrnRequestHelper trnRequestHelper,
    ICurrentUserProvider currentUserProvider,
    ITrnGenerationApiClient trnGenerationApiClient,
#pragma warning disable CS9113 // Parameter is unread.
    INameSynonymProvider nameSynonymProvider,
#pragma warning restore CS9113 // Parameter is unread.
    MessageSerializer messageSerializer,
    IClock clock)
{
    public async Task<ApiResult<TrnRequestInfo>> HandleAsync(CreateTrnRequestCommand command)
    {
        var (currentApplicationUserId, currentApplicationUserName) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await trnRequestHelper.GetTrnRequestInfoAsync(currentApplicationUserId, command.RequestId);
        if (trnRequest is not null)
        {
            return ApiError.TrnRequestAlreadyCreated(command.RequestId);
        }

        // Normalize names; DQT matching process requires a single-word first name :-|
        var firstAndMiddleNames = $"{command.FirstName} {command.MiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var firstName = firstAndMiddleNames.First();
        var middleName = string.Join(' ', firstAndMiddleNames.Skip(1));

        //var firstNameSynonyms = (await nameSynonymProvider.GetAllNameSynonyms()).GetValueOrDefault(firstName, []);
        var firstNameSynonyms = Array.Empty<string>();  // Disabled temporarily

        var normalizedNino = NationalInsuranceNumberHelper.Normalize(command.NationalInsuranceNumber);

        // Check workforce data for NINO matches
        var workforceDataMatches = normalizedNino is not null ?
            await dbContext.TpsEmployments
                .Where(e => e.NationalInsuranceNumber == normalizedNino)
                .Join(dbContext.Persons, e => e.PersonId, p => p.PersonId, (e, p) => p.DqtContactId!.Value)
                .ToArrayAsync() :
            [];

        var potentialDuplicates = (await crmQueryDispatcher.ExecuteQueryAsync(
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
            trn = await trnGenerationApiClient.GenerateTrnAsync();
        }

        var potentialDuplicatePersonIds = potentialDuplicates.Select(d => d.ContactId).ToList();
        var resultsWithActiveAlerts = await dbContext.Alerts
            .Where(a => potentialDuplicatePersonIds.Contains(a.PersonId) && a.IsOpen)
            .Select(a => a.PersonId)
            .Distinct()
            .ToArrayAsync();

        var emailAddress = command.EmailAddresses?.FirstOrDefault();

        var outboxMessages = new List<dfeta_TrsOutboxMessage>();
        if (command.OneLoginUserSubject is string oneLoginUserId)
        {
            outboxMessages.Add(messageSerializer.CreateCrmOutboxMessage(new TrnRequestMetadataMessage()
            {
                ApplicationUserId = currentApplicationUserId,
                RequestId = command.RequestId,
                CreatedOn = clock.UtcNow,
                IdentityVerified = command.IdentityVerified,
                OneLoginUserSubject = oneLoginUserId,
                Name = GetNonEmptyValues(command.FirstName, command.MiddleName, command.LastName),
                DateOfBirth = command.DateOfBirth,
                EmailAddress = emailAddress
            }));
        }

        var contactId = await crmQueryDispatcher.ExecuteQueryAsync(new CreateContactQuery()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = command.LastName,
            StatedFirstName = command.FirstName,
            StatedMiddleName = command.MiddleName ?? "",
            StatedLastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            Gender = command.Gender?.ConvertToContact_GenderCode() ?? Contact_GenderCode.Notavailable,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = NationalInsuranceNumberHelper.Normalize(command.NationalInsuranceNumber),
            PotentialDuplicates = potentialDuplicates.Select(d => (Duplicate: d, HasActiveAlert: resultsWithActiveAlerts.Contains(d.ContactId))).ToArray(),
            ApplicationUserName = currentApplicationUserName,
            Trn = trn,
            TrnRequestId = TrnRequestHelper.GetCrmTrnRequestId(currentApplicationUserId, command.RequestId),
            OutboxMessages = outboxMessages,
            Address1Line1 = command.AddressLine1,
            Address1Line2 = command.AddressLine2,
            Address1Line3 = command.AddressLine3,
            Address1City = command.City,
            Address1PostalCode = command.Postcode,
            Address1Country = command.Country
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

        static string[] GetNonEmptyValues(params string?[] values)
        {
            var result = new List<string>(values.Length);

            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }

            return result.ToArray();
        }
    }
}
