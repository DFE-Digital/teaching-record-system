using System.Diagnostics;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.PersonMatching;
using TeachingRecordSystem.Core.Services.TrnGeneration;
using TeachingRecordSystem.Core.Services.TrnRequests;
using Gender = TeachingRecordSystem.Core.Models.Gender;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record CreateTrnRequestCommand : ICommand<TrnRequestInfo>
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
    IPersonMatchingService personMatchingService,
    TrnRequestService trnRequestService,
    ICurrentUserProvider currentUserProvider,
    ITrnGenerator trnGenerator,
    IClock clock) :
    ICommandHandler<CreateTrnRequestCommand, TrnRequestInfo>
{
    public async Task<ApiResult<TrnRequestInfo>> ExecuteAsync(CreateTrnRequestCommand command)
    {
        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        var requestExists = await dbContext.TrnRequestMetadata.AnyAsync(m => m.ApplicationUserId == currentApplicationUserId && m.RequestId == command.RequestId);
        if (requestExists)
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
            Name = new[] { command.FirstName, command.MiddleName, command.LastName }.GetNonEmptyValues(),
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

            trn = await trnGenerator.GenerateTrnAsync();

            var createPersonResult = trnRequestService.CreatePersonFromTrnRequest(trnRequestMetadata, trn, now);
            dbContext.Persons.Add(createPersonResult.Person);

            var personCreatedEvent = new LegacyEvents.PersonCreatedEvent
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

        await trnRequestService.TryEnsureTrnTokenAsync(trnRequestMetadata, trn);

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
    }
}
