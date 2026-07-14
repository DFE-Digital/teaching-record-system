using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks;

public class SupportTaskService(TrsDbContext dbContext, IEventPublisher eventPublisher)
{
    public async Task<IReadOnlyCollection<AssignableUserInfo>> GetAssignableUsersAsync()
    {
        return await dbContext.Users
            .Where(u => u.Role == UserRoles.AccessManager || u.Role == UserRoles.RecordManager)
            .OrderBy(u => u.Name)
            .Select(u => new AssignableUserInfo(u.UserId, u.Name))
            .ToArrayAsync();
    }

    public async Task<SupportTaskNote> CreateNoteAsync(CreateSupportTaskNoteOptions options, ProcessContext processContext)
    {
        var note = new SupportTaskNote
        {
            SupportTaskNoteId = Guid.NewGuid(),
            SupportTaskReference = options.SupportTaskReference,
            Content = options.Content,
            CreatedOn = processContext.Now,
            CreatedByUserId = options.CreatedByUserId
        };

        dbContext.SupportTaskNotes.Add(note);
        await dbContext.SaveChangesAsync();

        await eventPublisher.PublishSingleEventAsync(
            new SupportTaskNoteCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskNote = EventModels.SupportTaskNote.FromModel(note)
            },
            processContext);

        return note;
    }

    public async Task<SupportTask> CreateSupportTaskAsync(CreateSupportTaskOptions options, ProcessContext processContext)
    {
        if (options.SupportTaskType.GetDataType() != options.Data.GetType())
        {
            throw new InvalidOperationException(
                $"{nameof(options.Data)} type '{options.Data.GetType()}' is not valid for the specified {nameof(SupportTaskType)}.");
        }

        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var supportTask = new SupportTask
        {
            CreatedOn = processContext.Now,
            UpdatedOn = processContext.Now,
            SupportTaskType = options.SupportTaskType,
            Data = options.Data,
            PersonId = options.PersonId,
            OneLoginUserSubject = options.OneLoginUserSubject,
            TrnRequestApplicationUserId = options.TrnRequest?.ApplicationUserId,
            TrnRequestId = options.TrnRequest?.RequestId,
            SubjectName = options.Subject.Name,
            SubjectEmailAddress = options.Subject.EmailAddress
        };

        dbContext.SupportTasks.Add(supportTask);

        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new SupportTaskCreatedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTask = EventModels.SupportTask.FromModel(supportTask)
            });

        return supportTask;
    }

    public async Task DeleteSupportTaskAsync(DeleteSupportTaskOptions options, ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var supportTask = await dbContext.SupportTasks.FindOrThrowAsync(options.SupportTaskReference);

        supportTask.DeletedOn = processContext.Now;

        await dbContext.SaveChangesAsync();

        await eventScope.PublishEventAsync(
            new SupportTaskDeletedEvent
            {
                EventId = Guid.NewGuid(),
                SupportTaskReference = options.SupportTaskReference,
                SupportTask = EventModels.SupportTask.FromModel(supportTask),
                ReasonDetail = options.ReasonDetail
            });
    }

    public async Task<bool> AllocateSupportTaskAsync(AllocateSupportTaskOptions options, ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var supportTask = await dbContext.SupportTasks.FindOrThrowAsync(options.SupportTaskReference);

        if (supportTask.Status is SupportTaskStatus.Closed)
        {
            throw new InvalidOperationException("Support task is closed.");
        }

        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);

        supportTask.Status = options.Status;
        supportTask.AssignedToUserId = options.AssignToUserId;

        var changes = SupportTaskUpdatedEventChanges.None |
            (supportTask.Status != oldSupportTaskEventModel.Status ? SupportTaskUpdatedEventChanges.Status : 0) |
            (supportTask.AssignedToUserId != oldSupportTaskEventModel.AssignedToUserId ? SupportTaskUpdatedEventChanges.AssignedToUserId : 0);

        if (changes is not SupportTaskUpdatedEventChanges.None)
        {
            supportTask.UpdatedOn = processContext.Now;

            await dbContext.SaveChangesAsync();

            await eventScope.PublishEventAsync(
                new SupportTaskUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    SupportTaskReference = supportTask.SupportTaskReference,
                    Changes = changes,
                    OldSupportTask = oldSupportTaskEventModel,
                    SupportTask = EventModels.SupportTask.FromModel(supportTask),
                    Comments = null,
                    RejectionReason = null
                });

            return true;
        }

        return false;
    }

    public Task UpdateSupportTaskAsync(UpdateSupportTaskOptions options, ProcessContext processContext)
    {
        return UpdateSupportTaskCoreAsync(options, updateAction: null, processContext);
    }

    public Task UpdateSupportTaskAsync<TData>(UpdateSupportTaskOptions<TData> options, ProcessContext processContext)
        where TData : ISupportTaskData, IEquatable<TData>
    {
        return UpdateSupportTaskCoreAsync(
            options,
            (supportTask, changes) =>
            {
                if (supportTask.SupportTaskType.GetDataType() != typeof(TData))
                {
                    throw new InvalidOperationException(
                        $"{typeof(TData).Name} is not valid for the specified support task's type.");
                }

                var oldData = supportTask.Data;
                supportTask.Data = options.UpdateData(supportTask.GetData<TData>());

                return changes | (!supportTask.GetData<TData>().Equals(oldData) ? SupportTaskUpdatedEventChanges.Data : 0);
            },
            processContext);
    }

    public async Task UpdateSupportTaskCoreAsync(
        UpdateSupportTaskOptions options,
        Func<SupportTask, SupportTaskUpdatedEventChanges, SupportTaskUpdatedEventChanges>? updateAction,
        ProcessContext processContext)
    {
        await using var eventScope = eventPublisher.GetOrCreateEventScope(processContext);

        var supportTask = await dbContext.SupportTasks.FindOrThrowAsync(options.SupportTaskReference);

        if (supportTask.Status is SupportTaskStatus.Closed)
        {
            throw new InvalidOperationException("Support task is closed.");
        }

        var oldSupportTaskEventModel = EventModels.SupportTask.FromModel(supportTask);

        supportTask.Status = options.Status;

        var changes = SupportTaskUpdatedEventChanges.None |
            (supportTask.Status != oldSupportTaskEventModel.Status ? SupportTaskUpdatedEventChanges.Status : 0);

        options.SavedJourneyState.Match(
            sjs =>
            {
                supportTask.ResolveJourneySavedState = sjs;
                changes |= SupportTaskUpdatedEventChanges.ResolveJourneySavedState;
            },
            () => { });

        if (updateAction is not null)
        {
            changes = updateAction(supportTask, changes);
        }

        // Derived from Data, so this must run after updateAction has applied any changes to it.
        if (options.Status is SupportTaskStatus.Closed)
        {
            supportTask.OutcomeLabel = supportTask.Data.GetOutcomeLabel();
            supportTask.CompletedOn = processContext.Now;
            supportTask.CompletedByUserId = processContext.Process.UserId;
        }
        else
        {
            supportTask.OutcomeLabel = null;
            supportTask.CompletedOn = null;
            supportTask.CompletedByUserId = null;
        }

        if (changes is not SupportTaskUpdatedEventChanges.None)
        {
            supportTask.UpdatedOn = processContext.Now;

            await dbContext.SaveChangesAsync();

            await eventScope.PublishEventAsync(
                new SupportTaskUpdatedEvent
                {
                    EventId = Guid.NewGuid(),
                    SupportTaskReference = supportTask.SupportTaskReference,
                    Changes = changes,
                    OldSupportTask = oldSupportTaskEventModel,
                    SupportTask = EventModels.SupportTask.FromModel(supportTask),
                    Comments = options.Comments,
                    RejectionReason = options.RejectionReason
                });
        }
    }
}

public record AssignableUserInfo(Guid UserId, string UserName);
