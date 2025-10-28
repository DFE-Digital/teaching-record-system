using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Services.SupportTasks;

public class SupportTaskService(TrsDbContext dbContext, IEventPublisher eventPublisher, IClock clock)
{
    public virtual async Task<DeleteSupportTaskResult> DeleteSupportTaskAsync(DeleteSupportTaskOptions options, ProcessContext processContext)
    {
        var supportTask = await dbContext.SupportTasks.FindAsync(options.SupportTaskReference);
        if (supportTask is null)
        {
            return DeleteSupportTaskResult.NotFound;
        }

        supportTask.DeletedOn = clock.UtcNow;
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new SupportTaskDeletedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = options.SupportTaskReference,
                SupportTask = EventModels.SupportTask.FromModel(supportTask),
                ReasonDetail = options.ReasonDetail
            },
            processContext);

        return DeleteSupportTaskResult.Ok;
    }
}
