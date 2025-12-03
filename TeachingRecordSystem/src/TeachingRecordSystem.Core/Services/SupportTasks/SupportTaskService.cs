using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

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

    public async Task<UpdateSupportTaskResult> UpdateSupportTaskAsync<TData>(UpdateSupportTaskOptions<TData> options, ProcessContext processContext)
        where TData : ISupportTaskData, IEquatable<TData>
    {
        var supportTask = await dbContext.SupportTasks.FindAsync(options.SupportTaskReference);

        if (supportTask is null)
        {
            return UpdateSupportTaskResult.NotFound;
        }

        if (supportTask.SupportTaskType.GetDataType() != typeof(TData))
        {
            throw new InvalidOperationException(
                $"{typeof(TData).Name} is not valid for the specified support task's type.");
        }

        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);

        supportTask.Status = options.Status;
        supportTask.Data = options.UpdateData(supportTask.GetData<TData>());

        var changes = SupportTaskUpdatedEventChanges.None |
            (supportTask.Status != oldSupportTaskEventModel.Status ? SupportTaskUpdatedEventChanges.Status : 0) |
            (!supportTask.GetData<TData>().Equals(oldSupportTaskEventModel.Data) ? SupportTaskUpdatedEventChanges.Data : 0);

        if (changes is not SupportTaskUpdatedEventChanges.None)
        {
            supportTask.UpdatedOn = processContext.Now;

            await dbContext.SaveChangesAsync();

            await eventPublisher.PublishEventAsync(
                new SupportTaskUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    SupportTaskReference = options.SupportTaskReference,
                    Changes = changes,
                    OldSupportTask = oldSupportTaskEventModel,
                    SupportTask = EventModels.SupportTask.FromModel(supportTask),
                    Comments = options.Comments
                },
                processContext);
        }

        return UpdateSupportTaskResult.Ok;
    }
}
