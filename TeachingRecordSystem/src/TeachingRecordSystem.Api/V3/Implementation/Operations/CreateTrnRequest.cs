using System.Diagnostics;
using System.Transactions;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Models.SupportTaskData;
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
    IClock clock)
{
    private static readonly TimeSpan _waitForBackgroundJobCompletion = TimeSpan.FromSeconds(10);

    public async Task<ApiResult<TrnRequestInfo>> HandleAsync(CreateTrnRequestCommand command)
    {
        var (currentApplicationUserId, _) = currentUserProvider.GetCurrentApplicationUser();

        var trnRequest = await trnRequestService.GetTrnRequestInfoAsync(currentApplicationUserId, command.RequestId);
        if (trnRequest is not null)
        {
            return ApiError.TrnRequestAlreadyCreated(command.RequestId);
        }

        // Create a TransactionScope so that enqueued Hangfire jobs are in the same transaction as our DB additions
        using var txn = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync();

        var normalizedNino = NationalInsuranceNumber.Normalize(command.NationalInsuranceNumber);
        var emailAddress = command.EmailAddresses.FirstOrDefault();

        var trnRequestMetadata = new PostgresModels.TrnRequestMetadata()
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
            EmailAddress = emailAddress,
            NationalInsuranceNumber = normalizedNino,
            Gender = (int?)command.Gender
        };

        var matchResult = await personMatchingService.MatchFromTrnRequestAsync(trnRequestMetadata);

        Guid? resolvedPersonId = null;
        string? trn = null;

        string? jobId = null;

        if (matchResult.Outcome is TrnRequestMatchResultOutcome.DefiniteMatch)
        {
            resolvedPersonId = matchResult.PersonId;
            trn = matchResult.Trn;
        }
        else if (matchResult.Outcome is TrnRequestMatchResultOutcome.PotentialMatches)
        {
            var supportTask = new PostgresModels.SupportTask()
            {
                SupportTaskReference = PostgresModels.SupportTask.GenerateSupportTaskReference(),
                CreatedOn = clock.UtcNow,
                UpdatedOn = clock.UtcNow,
                SupportTaskType = SupportTaskType.ApiTrnRequest,
                Status = SupportTaskStatus.Open,
                OneLoginUserSubject = command.OneLoginUserSubject,
                PersonId = null,
                TrnRequestApplicationUserId = currentApplicationUserId,
                TrnRequestId = command.RequestId,
                TrnRequestMetadata = trnRequestMetadata,
                Data = new ApiTrnRequestData()
            };

            dbContext.SupportTasks.Add(supportTask);
        }
        else
        {
            Debug.Assert(matchResult.Outcome is TrnRequestMatchResultOutcome.NoMatches);

            trn = await trnGenerationApiClient.GenerateTrnAsync();
            resolvedPersonId = Guid.NewGuid();

            Debug.Assert(resolvedPersonId is not null);
            Debug.Assert(trn is not null);

            jobId = await backgroundJobScheduler.EnqueueAsync<TrnRequestService>(
                h => h.CreateContactFromTrnRequestAsync(currentApplicationUserId, command.RequestId, resolvedPersonId.Value, trn));
        }

        var trnToken = emailAddress is not null && trn is not null ? await trnRequestService.CreateTrnTokenAsync(trn, emailAddress) : null;
        var aytqLink = trnToken is not null ? trnRequestService.GetAccessYourTeachingQualificationsLink(trnToken) : null;

        trnRequestMetadata.PotentialDuplicate = matchResult.Outcome is TrnRequestMatchResultOutcome.PotentialMatches;
        trnRequestMetadata.TrnToken = trnToken;
        trnRequestMetadata.ResolvedPersonId = resolvedPersonId;

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
            Trn = trn,
            Status = status,
            PotentialDuplicate = trnRequestMetadata.PotentialDuplicate!.Value,
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
}
