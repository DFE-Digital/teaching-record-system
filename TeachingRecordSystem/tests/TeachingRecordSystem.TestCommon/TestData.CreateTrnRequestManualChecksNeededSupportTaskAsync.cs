using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<SupportTask> CreateTrnRequestManualChecksNeededSupportTaskAsync(
        Guid? applicationUserId = null,
        SupportTaskStatus status = SupportTaskStatus.Open,
        DateTime? createdOn = null,
        Action<CreateApiTrnRequestSupportTaskBuilder>? configureApiTrnRequest = null)
    {
        var matchedPerson = await CreatePersonAsync(p => p.WithEmailAddress(GenerateUniqueEmail()).WithAlert().WithQts().WithEyts());

        if (applicationUserId is null)
        {
            var applicationUser = await CreateApplicationUserAsync();
            applicationUserId = applicationUser.UserId;
        }

        (var apiSupportTask, _, _) = await CreateResolvedApiTrnRequestSupportTaskAsync(
            applicationUserId.Value,
            matchedPerson.Person,
            t =>
            {
                t.WithTrnRequestStatus(TrnRequestStatus.Pending);
                configureApiTrnRequest?.Invoke(t);
            });

        return await CreateTrnRequestManualChecksNeededSupportTaskAsync(
            applicationUserId.Value,
            apiSupportTask.TrnRequestMetadata!.RequestId,
            status,
            createdOn);
    }

    public Task<SupportTask> CreateTrnRequestManualChecksNeededSupportTaskAsync(
        Guid trnRequestApplicationUserId,
        string trnRequestId,
        SupportTaskStatus status = SupportTaskStatus.Open,
        DateTime? createdOn = null)
    {
        return WithDbContextAsync(async dbContext =>
        {
            var task = SupportTask.Create(
                SupportTaskType.TrnRequestManualChecksNeeded,
                new TrnRequestManualChecksNeededData(),
                personId: null,
                oneLoginUserSubject: null,
                trnRequestApplicationUserId,
                trnRequestId,
                SystemUser.SystemUserId,
                (createdOn ?? Clock.UtcNow).ToUniversalTime(),
                out var createdEvent);
            task.Status = status;

            dbContext.SupportTasks.Add(task);
            dbContext.AddEventWithoutBroadcast(createdEvent);
            await dbContext.SaveChangesAsync();

            // Re-query what we've just added so we return a SupportTask with TrnRequestMetadata populated
            return await dbContext.SupportTasks
                .Include(t => t.TrnRequestMetadata)
                .ThenInclude(m => m!.ApplicationUser)
                .SingleAsync(t => t.SupportTaskReference == task.SupportTaskReference);
        });
    }
}
