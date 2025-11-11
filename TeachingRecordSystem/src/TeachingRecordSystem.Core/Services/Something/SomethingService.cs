using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrnRequests;
using PostgresModels = TeachingRecordSystem.Core.DataStore.Postgres.Models;
using PersonCreatedEvent = TeachingRecordSystem.Core.Events.Legacy.PersonCreatedEvent;

namespace TeachingRecordSystem.Core.Services.Something;

public record CreateTrnRequestInfo
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public required bool? IdentityVerified { get; init; }
    public required string? OneLoginUserSubject { get; init; }
    public required Gender? Gender { get; init; }
}

public record CreateTrnRequestResult
{
    public required TrnRequestStatus Status { get; init; }
    public required bool PotentialDuplicate { get; init; }
    public required string? Trn { get; init; }
    public required string? AytqLink { get; init; }
}

public record CreateTrnRequestInfo2
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required string? FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string? LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required string? EmailAddress { get; init; }
    public required string? NationalInsuranceNumber { get; init; }
    public string? PreviousFirstName { get; init; }
    public string? PreviousMiddleName { get; init; }
    public string? PreviousLastName { get; init; }
    public string? WorkEmailAddress { get; init; }
    public bool? NpqWorkingInEducationalSetting { get; init; }
    public string? NpqApplicationId { get; init; }
    public string? NpqName { get; init; }
    public string? NpqTrainingProvider { get; init; }
    public Guid? NpqEvidenceFileId { get; init; }
    public string? NpqEvidenceFileName { get; init; }
    public string? AddressLine1 { get; init; }
    public string? AddressLine2 { get; init; }
    public string? AddressLine3 { get; init; }
    public string? City { get; init; }
    public string? Postcode { get; init; }
    public string? Country { get; init; }
}

public record ResetTrnRequestInfo
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required string? Reason { get; set; }
}

public record ResetTrnRequestResult
{
    public required SupportTaskType SupportTaskType { get; init; }
    public required string SupportTaskReference { get; set; }
}

public record AllocateTrnIfNotExistInfo
{
    public required Guid ApplicationUserId { get; init; }
    public required string RequestId { get; init; }
    public required string FirstName { get; init; }
    public required string? MiddleName { get; init; }
    public required string LastName { get; init; }
    public required DateOnly DateOfBirth { get; init; }
    public required EmailAddress? EmailAddress { get; init; }
    public required NationalInsuranceNumber? NationalInsuranceNumber { get; init; }
    public required Gender? Gender { get; init; }
}

public enum AllocateTrnIfNotExistResultOutcome
{
    TrnAllocated,
    TrnNotAllocatedDueToPotentialDuplicates
}

public record AllocateTrnIfNotExistResult
{
    public required AllocateTrnIfNotExistResultOutcome Outcome { get; init; }
    public required string? AllocatedTrn { get; init; }
    public required string[] PotentialDuplicateTrns { get; init; }
    public required Guid? EmailId { get; init; }
}

public class TrnRequestAlreadyCreatedException(string requestId) : TrsException($"TRN request has already been created. TRN request ID: '{requestId}'")
{
    public string RequestId => requestId;
}

public class TrnRequestNotFoundException(string requestId) : TrsException($"TRN request was not found. TRN request ID: '{requestId}'")
{
    public string RequestId => requestId;
}

public class TrnRequestAlreadyHasOpenSupportTasksException(string requestId) : TrsException($"TRN request already has open support tasks. TRN request ID: '{requestId}'")
{
    public string RequestId => requestId;
}

public class TrsException(string message, Exception? innerException = null) : Exception(message, innerException);

public class SomethingService(
    TrsDbContext dbContext,
    IPersonMatchingService personMatchingService,
    TrnRequestService trnRequestService,
    ITrnGenerator trnGenerator,
    IEventPublisher eventPublisher,
    IClock clock)
{
    public const string FirstNamePersonalisationKey = "first name";
    public const string LastNamePersonalisationKey = "last name";
    public const string TrnPersonalisationKey = "trn";

    public async Task<CreateTrnRequestResult> CreateTrnRequestAsync(CreateTrnRequestInfo request)
    {
        var requestExists = await dbContext.TrnRequestMetadata.AnyAsync(m => m.ApplicationUserId == request.ApplicationUserId && m.RequestId == request.RequestId);
        if (requestExists)
        {
            throw new TrnRequestAlreadyCreatedException(request.RequestId);
        }

        var normalizedNino = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

        var now = clock.UtcNow;

        var trnRequestMetadata = new PostgresModels.TrnRequestMetadata()
        {
            ApplicationUserId = request.ApplicationUserId,
            RequestId = request.RequestId,
            CreatedOn = now,
            IdentityVerified = request.IdentityVerified,
            OneLoginUserSubject = request.OneLoginUserSubject,
            Name = new[] { request.FirstName, request.MiddleName, request.LastName }.GetNonEmptyValues(),
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            EmailAddress = request.EmailAddress,
            NationalInsuranceNumber = normalizedNino,
            Gender = request.Gender
        };

        var matchResult = await personMatchingService.MatchFromTrnRequestAsync(trnRequestMetadata);

        string? trn = null;

        if (matchResult.Outcome is TrnRequestMatchResultOutcome.DefiniteMatch)
        {
            trn = matchResult.Trn;

            var furtherChecksNeeded = await trnRequestService.RequiresFurtherChecksNeededSupportTaskAsync(
                matchResult.PersonId,
                request.ApplicationUserId);

            trnRequestMetadata.SetResolvedPerson(matchResult.PersonId, furtherChecksNeeded ? TrnRequestStatus.Pending : TrnRequestStatus.Completed);

            if (furtherChecksNeeded)
            {
                var furtherChecksSupportTask = PostgresModels.SupportTask.Create(
                    SupportTaskType.TrnRequestManualChecksNeeded,
                    new TrnRequestManualChecksNeededData(),
                    matchResult.PersonId,
                    request.OneLoginUserSubject,
                    request.ApplicationUserId,
                    request.RequestId,
                    createdBy: request.ApplicationUserId,
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
                request.OneLoginUserSubject,
                request.ApplicationUserId,
                request.RequestId,
                createdBy: request.ApplicationUserId,
                now,
                out var createdEvent);

            dbContext.SupportTasks.Add(supportTask);
            await dbContext.AddEventAndBroadcastAsync(createdEvent);
        }
        else
        {
            Debug.Assert(matchResult.Outcome is TrnRequestMatchResultOutcome.NoMatches);

            trn = await trnGenerator.GenerateTrnAsync();

            var createPersonResult = trnRequestService.CreatePersonFromTrnRequest(trnRequestMetadata, trn, now);
            dbContext.Persons.Add(createPersonResult.Person);

            var personCreatedEvent = new LegacyEvents.PersonCreatedEvent
            {
                EventId = Guid.NewGuid(),
                CreatedUtc = now,
                RaisedBy = request.ApplicationUserId,
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

        var trnToken = request.EmailAddress is not null && trn is not null ? await trnRequestService.CreateTrnTokenAsync(trn, request.EmailAddress) : null;
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

        await trnRequestService.TryEnsureTrnTokenAsync(trnRequestMetadata, trn);

        await dbContext.SaveChangesAsync();

        return new()
        {
            Status = trnRequestMetadata.Status!.Value,
            Trn = trn,
            PotentialDuplicate = trnRequestMetadata.PotentialDuplicate!.Value,
            AytqLink = aytqLink
        };
    }

    public async Task CreateTrnRequest2Async(CreateTrnRequestInfo2 request)
    {
        var trnRequestMetadata = new PostgresModels.TrnRequestMetadata
        {
            OneLoginUserSubject = null,
            CreatedOn = clock.UtcNow,
            RequestId = request.RequestId,
            IdentityVerified = false,
            ApplicationUserId = request.ApplicationUserId,
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            PreviousFirstName = request.PreviousFirstName,
            PreviousMiddleName = request.PreviousMiddleName,
            PreviousLastName = request.PreviousLastName,
            WorkEmailAddress = request.WorkEmailAddress,
            Name = new[] { request.FirstName, request.MiddleName, request.LastName }.GetNonEmptyValues(),
            EmailAddress = request.EmailAddress,
            DateOfBirth = request.DateOfBirth,
            NationalInsuranceNumber = Core.NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber),
            NpqApplicationId = request.NpqApplicationId,
            NpqName = request.NpqName,
            NpqTrainingProvider = request.NpqTrainingProvider,
            AddressLine1 = request.AddressLine1,
            AddressLine2 = request.AddressLine2,
            Postcode = request.Postcode,
            City = request.City,
            Country = request.Country,
            NpqEvidenceFileId = request.NpqEvidenceFileId,
            NpqEvidenceFileName = request.NpqEvidenceFileName,
            NpqWorkingInEducationalSetting = request.NpqWorkingInEducationalSetting
        };

        // look for potential matches
        var matchResult = await personMatchingService.MatchFromTrnRequestAsync(trnRequestMetadata);
        trnRequestMetadata.PotentialDuplicate = matchResult.Outcome is not TrnRequestMatchResultOutcome.NoMatches;

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

        var supportTask = PostgresModels.SupportTask.Create(
            supportTaskType: SupportTaskType.NpqTrnRequest,
            data: new NpqTrnRequestData(),
            personId: null,
            oneLoginUserSubject: null,
            trnRequestApplicationUserId: request.ApplicationUserId,
            trnRequestId: request.RequestId,
            createdBy: request.ApplicationUserId,
            now: clock.UtcNow,
            out var createdEvent
            );
        dbContext.SupportTasks.Add(supportTask);
        await dbContext.AddEventAndBroadcastAsync(createdEvent);
        dbContext.SaveChanges();
    }

    public async Task<ResetTrnRequestResult> ResetTrnRequestAsync(ResetTrnRequestInfo request)
    {
        var requestMetadata = await dbContext.TrnRequestMetadata
    .SingleOrDefaultAsync(r => r.ApplicationUserId == request.ApplicationUserId && r.RequestId == request.RequestId);

        if (requestMetadata is null)
        {
            throw new TrnRequestNotFoundException(request.RequestId);
        }

        // Check if there are any Open support tasks already for this request - we can't reset if there are
        var haveSupportTasksForRequest = await dbContext.SupportTasks
            .Where(t => t.TrnRequestApplicationUserId == request.ApplicationUserId && t.TrnRequestId == request.RequestId)
            .Where(t => t.Status == SupportTaskStatus.Open)
            .AnyAsync();

        if (haveSupportTasksForRequest)
        {
            throw new TrnRequestAlreadyHasOpenSupportTasksException(request.RequestId);
        }

        var now = clock.UtcNow;

        var matchResult = await personMatchingService.MatchFromTrnRequestAsync(requestMetadata);

        // We only handle PotentialMatches for now
        if (matchResult.Outcome is not TrnRequestMatchResultOutcome.PotentialMatches)
        {
            throw new NotImplementedException();
        }

        var processContext = new ProcessContext(ProcessType.TrnRequestResetting, now, SystemUser.SystemUserId);

        var oldTrnRequest = EventModels.TrnRequestMetadata.FromModel(requestMetadata);
        requestMetadata.Reset();

        var changes = (oldTrnRequest.Status != requestMetadata.Status ? TrnRequestUpdatedChanges.Status : 0) |
            (oldTrnRequest.ResolvedPersonId != requestMetadata.ResolvedPersonId ? TrnRequestUpdatedChanges.ResolvedPersonId : 0);

        var supportTask = SupportTask.Create(
            SupportTaskType.ApiTrnRequest,
            new ApiTrnRequestData(),
            personId: null,
            requestMetadata.OneLoginUserSubject,
            requestMetadata.ApplicationUserId,
            requestMetadata.RequestId,
            createdBy: SystemUser.SystemUserId,
            now,
            out _);

        dbContext.SupportTasks.Add(supportTask);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            },
            processContext);

        await eventPublisher.PublishEventAsync(
            new TrnRequestUpdatedEvent
            {
                EventId = Guid.NewGuid(),
                SourceApplicationUserId = request.ApplicationUserId,
                RequestId = request.RequestId,
                Changes = changes,
                TrnRequest = EventModels.TrnRequestMetadata.FromModel(requestMetadata),
                OldTrnRequest = oldTrnRequest,
                ReasonDetails = request.Reason
            },
            processContext);

        return new()
        {
            SupportTaskReference = supportTask.SupportTaskReference,
            SupportTaskType = supportTask.SupportTaskType
        };
    }

    public async Task<AllocateTrnIfNotExistResult> AllocateTrnIfNotExistsAsync(AllocateTrnIfNotExistInfo request)
    {
        var now = clock.UtcNow;

        var trnRequestMetadata = new PostgresModels.TrnRequestMetadata
        {
            ApplicationUserId = request.ApplicationUserId,
            RequestId = request.RequestId,
            CreatedOn = now,
            IdentityVerified = null,
            OneLoginUserSubject = null,
            Name = new[] { request.FirstName, request.MiddleName, request.LastName }.GetNonEmptyValues(),
            FirstName = request.FirstName,
            MiddleName = request.MiddleName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            EmailAddress = request.EmailAddress?.ToString(),
            NationalInsuranceNumber = request.NationalInsuranceNumber?.ToString(),
            Gender = request.Gender
        };

        var personMatchingResult = await personMatchingService.MatchFromTrnRequestAsync(trnRequestMetadata);

        switch (personMatchingResult.Outcome)
        {
            case TrnRequestMatchResultOutcome.NoMatches:
                var newTrn = await trnGenerator.GenerateTrnAsync();

                var person = PostgresModels.Person.Create(
                    newTrn,
                    request.FirstName!,
                    request.MiddleName ?? "",
                    request.LastName!,
                    request.DateOfBirth,
                    request.EmailAddress,
                    request.NationalInsuranceNumber,
                    request.Gender,
                    now,
                    sourceTrnRequest: (request.ApplicationUserId, request.RequestId));

                dbContext.Persons.Add(person.Person);

                dbContext.TrnRequestMetadata.Add(trnRequestMetadata);

                dbContext.AddEventWithoutBroadcast(new PersonCreatedEvent
                {
                    EventId = Guid.NewGuid(),
                    CreatedUtc = now,
                    RaisedBy = SystemUser.SystemUserId,
                    PersonId = person.Person.PersonId,
                    PersonAttributes = person.PersonAttributes,
                    CreateReason = null,
                    CreateReasonDetail = null,
                    EvidenceFile = null,
                    TrnRequestMetadata = EventModels.TrnRequestMetadata.FromModel(trnRequestMetadata),

                });

                var emailId = Guid.NewGuid();

                dbContext.Emails.Add(new PostgresModels.Email
                {
                    EmailId = emailId,
                    TemplateId = EmailTemplateIds.TrnGeneratedForNpq,
                    EmailAddress = request.EmailAddress!.ToString(),
                    Personalization = new Dictionary<string, string>()
                    {
                        [FirstNamePersonalisationKey] = request.FirstName,
                        [LastNamePersonalisationKey] = request.LastName,
                        [TrnPersonalisationKey] = newTrn!,
                    },
                });

                await dbContext.SaveChangesAsync();

                return new()
                {
                    Outcome = AllocateTrnIfNotExistResultOutcome.TrnAllocated,
                    AllocatedTrn = newTrn,
                    EmailId = emailId,
                    PotentialDuplicateTrns = []
                };

            default:
                var trns = await dbContext.Persons
                    .Where(p => personMatchingResult.PotentialMatchesPersonIds.Contains(p.PersonId))
                    .OrderBy(p => p.Trn)
                    .Select(p => p.Trn)
                    .ToArrayAsync();

                return new()
                {
                    Outcome = AllocateTrnIfNotExistResultOutcome.TrnNotAllocatedDueToPotentialDuplicates,
                    PotentialDuplicateTrns = trns.GetNonEmptyValues(),
                    AllocatedTrn = null,
                    EmailId = null,
                };
        }
    }
}
