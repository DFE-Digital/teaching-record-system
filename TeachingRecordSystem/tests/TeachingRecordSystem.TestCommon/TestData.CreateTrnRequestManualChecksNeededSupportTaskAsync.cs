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
            var supportTaskReference = SupportTask.GenerateSupportTaskReference();

            var metadata =
                await dbContext.TrnRequestMetadata.SingleAsync(m => m.ApplicationUserId == trnRequestApplicationUserId && m.RequestId == trnRequestId);

            dbContext.SupportTasks.Add(new SupportTask
            {
                SupportTaskReference = supportTaskReference,
                CreatedOn = Clock.UtcNow,
                UpdatedOn = Clock.UtcNow,
                SupportTaskType = SupportTaskType.TrnRequestManualChecksNeeded,
                Status = status,
                Data = new TrnRequestManualChecksNeededData(),
                TrnRequestApplicationUserId = trnRequestApplicationUserId,
                TrnRequestId = trnRequestId,
                TrnRequestMetadata = metadata
            });

            await dbContext.SaveChangesAsync();

            return await dbContext.SupportTasks
                .Include(t => t.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == supportTaskReference);
        });
    }
}
