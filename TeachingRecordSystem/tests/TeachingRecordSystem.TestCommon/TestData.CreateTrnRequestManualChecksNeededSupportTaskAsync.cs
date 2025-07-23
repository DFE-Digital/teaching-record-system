using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<SupportTask> CreateTrnRequestManualChecksNeededSupportTaskAsync(
        Guid trnRequestApplicationUserId,
        string trnRequestId,
        SupportTaskStatus status = SupportTaskStatus.Open)
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
                Clock.UtcNow,
                out var createdEvent);
            task.Status = status;

            dbContext.SupportTasks.Add(task);
            dbContext.AddEventWithoutBroadcast(createdEvent);
            await dbContext.SaveChangesAsync();

            // Re-query what we've just added so we return a SupportTask with TrnRequestMetadata populated
            return await dbContext.SupportTasks
                .Include(t => t.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == task.SupportTaskReference);
        });
    }
}
