using System.Diagnostics;
using System.Text;
using System.Transactions;
using Microsoft.Extensions.Options;
using Optional;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Jobs.Scheduling;
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
    IDbContextFactory<TrsDbContext> dbContextFactory,
    IPersonMatchingService personMatchingService,
    TrnRequestService trnRequestService,
    ICurrentUserProvider currentUserProvider,
    ITrnGenerator trnGenerationApiClient,
    IBackgroundJobScheduler backgroundJobScheduler,
    IClock clock,
    IFeatureProvider featureProvider)
{
    private static readonly TimeSpan _waitForBackgroundJobCompletion = TimeSpan.FromSeconds(10);

    public async Task<ApiResult<TrnRequestInfo>> HandleAsync(CreateTrnRequestCommand command)
    {
        var contactsMigrated = featureProvider.IsEnabled(FeatureNames.ContactsMigrated);

        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        // Create a TransactionScope so that enqueued Hangfire jobs are in the same transaction as our DB additions.
        // We can remove this once contacts are migrated to TRS (as we won't need Hangfire jobs here from that point)
        // as we as moving to injecting a TrsDbContext instead of a IDbContextFactory<TrsDbContext>.
        using var txn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

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
        string? jobId = null;

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

            if (contactsMigrated)
            {
                var (newPerson, _) = trnRequestService.CreatePersonFromTrnRequest(trnRequestMetadata, trn, now);

                // TODO Event

                dbContext.Persons.Add(newPerson);

                trnRequestMetadata.SetResolvedPerson(newPerson.PersonId);
            }
            else
            {
                var newContactId = Guid.NewGuid();
                trnRequestMetadata.SetResolvedPerson(newContactId);

                jobId = await backgroundJobScheduler.EnqueueAsync<TrnRequestService>(
                    h => h.CreateContactFromTrnRequestAsync(dbContext, currentApplicationUserId, command.RequestId, newContactId, trn));
            }
        }

        var trnToken = emailAddress is not null && trn is not null ? await trnRequestService.CreateTrnTokenAsync(trn, emailAddress) : null;
        var aytqLink = trnToken is not null ? trnRequestService.GetAccessYourTeachingQualificationsLink(trnToken) : null;

        trnRequestMetadata.PotentialDuplicate = matchResult.Outcome is TrnRequestMatchResultOutcome.PotentialMatches;
        trnRequestMetadata.TrnToken = trnToken;

        trnRequestMetadata.Matches = new PostgresModels.TrnRequestMatches()
        {
            MatchedRecords = matchResult.Outcome switch
            {
                TrnRequestMatchResultOutcome.PotentialMatches =>
                    matchResult.PotentialMatchesPersonIds
                        .Select(id => new PostgresModels.TrnRequestMatchedRecord() { PersonId = id })
                        .ToList(),
                TrnRequestMatchResultOutcome.DefiniteMatch => [new PostgresModels.TrnRequestMatchedRecord() { PersonId = matchResult.PersonId }],
                _ => []
            }
        };

        dbContext.TrnRequestMetadata.Add(trnRequestMetadata);

        await dbContext.SaveChangesAsync();
        txn.Complete();

        // This explicit Dispose() is to trigger the TransactionCompletedEvent *before* we call backgroundJobScheduler.WaitForJobToCompleteAsync()
        txn.Dispose();

        if (jobId is not null)
        {
            using var backgroundJobCompletionCts = new CancellationTokenSource();
            backgroundJobCompletionCts.CancelAfter(_waitForBackgroundJobCompletion);

            await backgroundJobScheduler.WaitForJobToCompleteAsync(jobId, backgroundJobCompletionCts.Token)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
        }

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
}
