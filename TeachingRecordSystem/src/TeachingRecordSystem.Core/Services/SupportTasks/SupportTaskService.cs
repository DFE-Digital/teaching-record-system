using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks;

public class SupportTaskService(TrsDbContext dbContext, IEventPublisher eventPublisher)
{
    public async Task<SupportTask> CreateSupportTaskAsync(CreateSupportTaskOptions options, ProcessContext processContext)
    {
        if (options.SupportTaskType.GetDataType() != options.Data.GetType())
        {
            throw new InvalidOperationException(
                $"{nameof(options.Data)} type '{options.Data.GetType()}' is not valid for the specified {nameof(SupportTaskType)}.");
        }

        var supportTask = new SupportTask
        {
            SupportTaskReference = SupportTask.GenerateSupportTaskReference(),
            CreatedOn = processContext.Now,
            UpdatedOn = processContext.Now,
            SupportTaskType = options.SupportTaskType,
            Status = SupportTaskStatus.Open,
            Data = options.Data,
            PersonId = options.PersonId,
            OneLoginUserSubject = options.OneLoginUserSubject,
            TrnRequestApplicationUserId = options.TrnRequest?.ApplicationUserId,
            TrnRequestId = options.TrnRequest?.RequestId
        };

        dbContext.SupportTasks.Add(supportTask);

        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishEventAsync(
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            },
            processContext);

        return supportTask;
    }

    public async Task<DeleteSupportTaskResult> DeleteSupportTaskAsync(DeleteSupportTaskOptions options, ProcessContext processContext)
    {
        var supportTask = await dbContext.SupportTasks.FindAsync(options.SupportTaskReference);
        if (supportTask is null)
        {
            return DeleteSupportTaskResult.NotFound;
        }

        supportTask.DeletedOn = processContext.Now;

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
