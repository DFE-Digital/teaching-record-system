using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Options;
using Optional;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Models.SupportTaskData;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrnRequests;
using Gender = TeachingRecordSystem.Core.Models.Gender;

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
}

public class CreateTrnRequestHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    IPersonMatchingService personMatchingService,
    TrnRequestService trnRequestService,
    ICurrentUserProvider currentUserProvider,
    ITrnGenerator trnGenerationApiClient,
    IClock clock,
    IOptions<TrnRequestOptions> trnRequestOptionsAccessor,
    IFeatureProvider featureProvider)
{
    public async Task<ApiResult<TrnRequestInfo>> HandleAsync(CreateTrnRequestCommand command)
    {
        if (!featureProvider.IsEnabled(FeatureNames.ContactsMigrated))
        {
            return await HandleOverDqtAsync(command);
        }

        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await trnRequestService.GetTrnRequestInfoAsync(dbContext, currentApplicationUserId, command.RequestId);
        if (trnRequest is not null)
        {
            return ApiError.TrnRequestAlreadyCreated(command.RequestId);
        }

        var normalizedNino = NationalInsuranceNumber.Normalize(command.NationalInsuranceNumber);
        var emailAddress = command.EmailAddresses.FirstOrDefault();

        var now = clock.UtcNow;

        var trnRequestMetadata = new PostgresModels.TrnRequestMetadata()
        {
            ApplicationUserId = currentApplicationUserId,
            RequestId = command.RequestId,
            CreatedOn = now,
            IdentityVerified = command.IdentityVerified,
            OneLoginUserSubject = command.OneLoginUserSubject,
            Name = GetNonEmptyValues(
                command.FirstName,
                command.MiddleName,
                command.LastName),
            FirstName = command.FirstName,
            MiddleName = command.MiddleName,
            LastName = command.LastName,
            DateOfBirth = command.DateOfBirth,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = normalizedNino,
            Gender = command.Gender
        };

        var matchResult = await personMatchingService.MatchFromTrnRequestAsync(trnRequestMetadata);

        string? trn = null;

        if (matchResult.Outcome is TrnRequestMatchResultOutcome.DefiniteMatch)
        {
            trn = matchResult.Trn;

            var furtherChecksNeeded = await trnRequestService.RequiresFurtherChecksNeededSupportTaskAsync(
                dbContext,
                matchResult.PersonId,
                currentApplicationUserId);

            trnRequestMetadata.SetResolvedPerson(matchResult.PersonId, furtherChecksNeeded ? TrnRequestStatus.Pending : TrnRequestStatus.Completed);

            if (furtherChecksNeeded)
            {
                var furtherChecksSupportTask = PostgresModels.SupportTask.Create(
                    SupportTaskType.TrnRequestManualChecksNeeded,
                    new TrnRequestManualChecksNeededData(),
                    matchResult.PersonId,
                    command.OneLoginUserSubject,
                    currentApplicationUserId,
                    command.RequestId,
                    createdBy: currentApplicationUserId,
                    now,
                    out var furtherChecksSupportTaskCreatedEvent);

                dbContext.SupportTasks.Add(furtherChecksSupportTask);
                await dbContext.AddEventAndBroadcastAsync(furtherChecksSupportTaskCreatedEvent);
            }
        }
        else if (matchResult.Outcome is TrnRequestMatchResultOutcome.PotentialMatches)
        {
            var supportTask = PostgresModels.SupportTask.Create(
                SupportTaskType.ApiTrnRequest,
                new ApiTrnRequestData(),
                personId: null,
                command.OneLoginUserSubject,
                currentApplicationUserId,
                command.RequestId,
                createdBy: currentApplicationUserId,
                now,
                out var createdEvent);

            dbContext.SupportTasks.Add(supportTask);
            await dbContext.AddEventAndBroadcastAsync(createdEvent);
        }
        else
        {
            Debug.Assert(matchResult.Outcome is TrnRequestMatchResultOutcome.NoMatches);

            trn = await trnGenerationApiClient.GenerateTrnAsync();

            var createPersonResult = trnRequestService.CreatePersonFromTrnRequest(trnRequestMetadata, trn, now);
            dbContext.Persons.Add(createPersonResult.Person);

            var personCreatedEvent = new PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = currentApplicationUserId,
                PersonId = createPersonResult.Person.PersonId,
                PersonAttributes = createPersonResult.PersonAttributes,
                CreateReason = null,
                CreateReasonDetail = null,
                EvidenceFile = null,
                TrnRequestMetadata = EventModels.TrnRequestMetadata.FromModel(trnRequestMetadata)
            };
            await dbContext.AddEventAndBroadcastAsync(personCreatedEvent);

            trnRequestMetadata.SetResolvedPerson(createPersonResult.Person.PersonId);
        }

        var trnToken = emailAddress is not null && trn is not null ? await trnRequestService.CreateTrnTokenAsync(trn, emailAddress) : null;
        var aytqLink = trnToken is not null ? trnRequestService.GetAccessYourTeachingQualificationsLink(trnToken) : null;

        trnRequestMetadata.PotentialDuplicate = matchResult.Outcome is TrnRequestMatchResultOutcome.PotentialMatches;
        trnRequestMetadata.TrnToken = trnToken;

        trnRequestMetadata.Matches = new PostgresModels.TrnRequestMatches()
        {
            MatchedPersons = matchResult.Outcome switch
            {
                TrnRequestMatchResultOutcome.PotentialMatches =>
                    matchResult.PotentialMatchesPersonIds
                        .Select(id => new PostgresModels.TrnRequestMatchedPerson() { PersonId = id })
                        .ToList(),
                TrnRequestMatchResultOutcome.DefiniteMatch => [new PostgresModels.TrnRequestMatchedPerson() { PersonId = matchResult.PersonId }],
                _ => []
            }
        };

        dbContext.TrnRequestMetadata.Add(trnRequestMetadata);

        await dbContext.SaveChangesAsync();

        var status = trnRequestMetadata.Status!.Value;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
#pragma warning disable TRS0001
            Person = new TrnRequestInfoPerson()
            {
                FirstName = command.FirstName,
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                EmailAddress = emailAddress,
                DateOfBirth = command.DateOfBirth,
                NationalInsuranceNumber = command.NationalInsuranceNumber
            },
#pragma warning restore TRS0001
            Trn = status is TrnRequestStatus.Completed ? trn : null,
            Status = status,
            PotentialDuplicate = trnRequestMetadata.PotentialDuplicate!.Value,
            AccessYourTeachingQualificationsLink = status is TrnRequestStatus.Completed ? aytqLink : null
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

    private async Task<ApiResult<TrnRequestInfo>> HandleOverDqtAsync(CreateTrnRequestCommand command)
    {
        var (currentApplicationUserId, currentApplicationUserName) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await trnRequestService.GetTrnRequestInfoAsync(dbContext, currentApplicationUserId, command.RequestId);
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

        var normalizedNino = NationalInsuranceNumber.Normalize(command.NationalInsuranceNumber);
        var emailAddress = command.EmailAddresses.FirstOrDefault();
        string? trnToken;
        string? aytqLink;

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

        TrnRequestMetadataMessage CreateMetadataOutboxMessage(bool potentialDuplicate, Guid? resolvedPersonId, string? trnToken) =>
            new TrnRequestMetadataMessage
            {
                ApplicationUserId = currentApplicationUserId,
                RequestId = command.RequestId,
                CreatedOn = clock.UtcNow,
                IdentityVerified = command.IdentityVerified,
                OneLoginUserSubject = command.OneLoginUserSubject,
                Name = GetNonEmptyValues(
                    command.FirstName,
                    command.MiddleName,
                    command.LastName),
                FirstName = command.FirstName,
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                DateOfBirth = command.DateOfBirth,
                PotentialDuplicate = potentialDuplicate,
                EmailAddress = emailAddress,
                NationalInsuranceNumber = command.NationalInsuranceNumber,
                Gender = command.Gender,
                AddressLine1 = null,
                AddressLine2 = null,
                AddressLine3 = null,
                City = null,
                Postcode = null,
                Country = null,
                TrnToken = trnToken,
                ResolvedPersonId = resolvedPersonId
            };

        // If any record has matched on NINO & DOB treat that as a definite match and return the existing record's details
        var matchedOnNinoAndDob = potentialDuplicates
            .Where(d => d.MatchedAttributes.Contains(Contact.Fields.dfeta_NINumber) && d.MatchedAttributes.Contains(Contact.Fields.BirthDate))
            .ToArray();

        if (matchedOnNinoAndDob is [var singleMatchOnNinoAndDob])
        {
            // FUTURE: consider whether we should be updating any missing attributes here

            trnToken = emailAddress is not null ? await trnRequestService.CreateTrnTokenAsync(singleMatchOnNinoAndDob.Trn, emailAddress) : null;
            aytqLink = trnToken is not null ? trnRequestService.GetAccessYourTeachingQualificationsLink(trnToken) : null;

            await crmQueryDispatcher.ExecuteQueryAsync(
                new CreateDqtOutboxMessageQuery(CreateMetadataOutboxMessage(potentialDuplicate: false, singleMatchOnNinoAndDob.ContactId, trnToken)));

            return new TrnRequestInfo()
            {
                RequestId = command.RequestId,
#pragma warning disable TRS0001
                Person = new TrnRequestInfoPerson()
                {
                    FirstName = command.FirstName,
                    MiddleName = command.MiddleName,
                    LastName = command.LastName,
                    EmailAddress = emailAddress,
                    DateOfBirth = command.DateOfBirth,
                    NationalInsuranceNumber = command.NationalInsuranceNumber
                },
#pragma warning restore TRS0001
                Trn = singleMatchOnNinoAndDob.Trn,
                Status = TrnRequestStatus.Completed,
                PotentialDuplicate = false,
                AccessYourTeachingQualificationsLink = aytqLink
            };
        }

        string? trn = null;
        var potentialDuplicate = potentialDuplicates.Count > 0;
        if (!potentialDuplicate)
        {
            trn = await trnGenerationApiClient.GenerateTrnAsync();
        }

        var potentialDuplicatePersonIds = potentialDuplicates.Select(d => d.ContactId).ToList();
        var potentialDuplicatePersons = (await dbContext.Persons
            .Where(a => potentialDuplicatePersonIds.Contains(a.PersonId))
            .Select(p => new { p.PersonId, HasOpenAlert = p.Alerts!.Any(a => a.IsOpen), p.QtsDate, p.EytsDate })
            .Distinct()
            .ToArrayAsync()).ToDictionary(p => p.PersonId, p => p);

        var reviewTasks = potentialDuplicates
            .Select(d => (
                Duplicate: d,
                HasActiveAlert: potentialDuplicatePersons[d.ContactId].HasOpenAlert,
                HasQts: potentialDuplicatePersons[d.ContactId].QtsDate is not null,
                HasEyts: potentialDuplicatePersons[d.ContactId].EytsDate is not null
            ))
            .Select(d => CreateDuplicateReviewTaskEntity(currentApplicationUserName, d.Duplicate, d.HasActiveAlert, d.HasQts, d.HasEyts))
            .ToArray();

        var allowContactPiiUpdatesFromUserIds = trnRequestOptionsAccessor.Value.AllowContactPiiUpdatesFromUserIds;

        trnToken = emailAddress is not null && trn is not null ? await trnRequestService.CreateTrnTokenAsync(trn, emailAddress) : null;
        aytqLink = trnToken is not null ? trnRequestService.GetAccessYourTeachingQualificationsLink(trnToken) : null;

        var contactId = Guid.NewGuid();

        var metadataMessage = CreateMetadataOutboxMessage(
            potentialDuplicate,
            resolvedPersonId: trn is not null ? contactId : null,
            trnToken);

        await crmQueryDispatcher.ExecuteQueryAsync(new CreateContactQuery()
        {
            ContactId = contactId,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = command.LastName,
            StatedFirstName = Option.Some(command.FirstName),
            StatedMiddleName = Option.Some(command.MiddleName ?? ""),
            StatedLastName = Option.Some(command.LastName),
            DateOfBirth = command.DateOfBirth,
            Gender = command.Gender?.ToContact_GenderCode() ?? Contact_GenderCode.Notavailable,
            EmailAddress = emailAddress,
            NationalInsuranceNumber = NationalInsuranceNumber.Normalize(command.NationalInsuranceNumber),
            ReviewTasks = reviewTasks,
            ApplicationUserName = currentApplicationUserName,
            Trn = trn,
            TrnRequestId = TrnRequestService.GetCrmTrnRequestId(currentApplicationUserId, command.RequestId),
            TrnRequestMetadataMessage = metadataMessage,
            AllowPiiUpdates = allowContactPiiUpdatesFromUserIds.Contains(currentApplicationUserId)
        });

        var status = trn is not null ? TrnRequestStatus.Completed : TrnRequestStatus.Pending;

        return new TrnRequestInfo()
        {
            RequestId = command.RequestId,
#pragma warning disable TRS0001
            Person = new TrnRequestInfoPerson()
            {
                FirstName = command.FirstName,
                MiddleName = command.MiddleName,
                LastName = command.LastName,
                EmailAddress = emailAddress,
                DateOfBirth = command.DateOfBirth,
                NationalInsuranceNumber = command.NationalInsuranceNumber
            },
#pragma warning restore TRS0001
            Trn = trn,
            Status = status,
            PotentialDuplicate = potentialDuplicate,
            AccessYourTeachingQualificationsLink = aytqLink
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

    private CreateContactQueryDuplicateReviewTask CreateDuplicateReviewTaskEntity(
        string applicationUserName,
        FindPotentialDuplicateContactsResult duplicate,
        bool hasActiveAlert,
        bool hasQts,
        bool hasEyts)
    {
        var description = GetDescription();

        var category = $"TRN request from {applicationUserName}";

        return new CreateContactQueryDuplicateReviewTask()
        {
            PotentialDuplicateContactId = duplicate.ContactId,
            Category = category,
            Subject = "Notification for QTS Unit Team",
            Description = description
        };

        string GetDescription()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Potential duplicate");
            sb.AppendLine("Matched on");

            foreach (var matchedAttribute in duplicate.MatchedAttributes)
            {
                sb.AppendLine(matchedAttribute switch
                {
                    Contact.Fields.FirstName => $"  - First name: '{duplicate.FirstName}'",
                    Contact.Fields.MiddleName => $"  - Middle name: '{duplicate.MiddleName}'",
                    Contact.Fields.LastName => $"  - Last name: '{duplicate.LastName}'",
                    Contact.Fields.dfeta_PreviousLastName => $"  - Previous last name: '{duplicate.PreviousLastName}'",
                    Contact.Fields.BirthDate => $"  - Date of birth: '{duplicate.DateOfBirth:dd/MM/yyyy}'",
                    Contact.Fields.dfeta_NINumber => $"  - National Insurance number: '{duplicate.NationalInsuranceNumber}'",
                    Contact.Fields.EMailAddress1 => $"  - Email address: '{duplicate.EmailAddress}'",
                    _ => throw new Exception($"Unknown matched field: '{matchedAttribute}'.")
                });
            }

            var additionalFlags = new List<string>();

            if (hasActiveAlert)
            {
                additionalFlags.Add("active sanctions");
            }

            if (hasQts)
            {
                additionalFlags.Add("QTS date");
            }

            if (hasEyts)
            {
                additionalFlags.Add("EYTS date");
            }

            if (additionalFlags.Count > 0)
            {
                sb.AppendLine($"Matched record has {string.Join(" & ", additionalFlags)}");
            }

            return sb.ToString();
        }
    }
}
