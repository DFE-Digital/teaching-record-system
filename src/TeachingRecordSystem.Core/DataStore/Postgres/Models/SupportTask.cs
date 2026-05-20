using EntityFrameworkCore.Projectables;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class SupportTask
{
    public string SupportTaskReference { get; } = null!;
    public required DateTime CreatedOn { get; init; }
    public required DateTime UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    public required SupportTaskType SupportTaskType { get; init; }
    public required SupportTaskStatus Status { get; set; }
    public string? OneLoginUserSubject { get; init; }
    public OneLoginUser? OneLoginUser { get; }
    public Guid? PersonId { get; init; }
    public Person? Person { get; }
    public Guid? TrnRequestApplicationUserId { get; init; }
    public string? TrnRequestId { get; init; }
    public TrnRequestMetadata? TrnRequestMetadata { get; }
    public required ISupportTaskData Data { get; set; }
    public SavedJourneyState? ResolveJourneySavedState { get; set; }

    [Projectable]
    public bool IsOutstanding => Status != SupportTaskStatus.Closed;

    public static SupportTask Create(
        SupportTaskType supportTaskType,
        ISupportTaskData data,
        Guid? personId,
        string? oneLoginUserSubject,
        Guid? trnRequestApplicationUserId,
        string? trnRequestId,
        EventModels.RaisedByUserInfo createdBy,
        DateTime now,
        out LegacyEvents.SupportTaskCreatedEvent createdEvent)
    {
        var task = new SupportTask
        {
            CreatedOn = now,
            UpdatedOn = now,
            SupportTaskType = supportTaskType,
            Status = SupportTaskStatus.Open,
            Data = data,
            PersonId = personId,
            OneLoginUserSubject = oneLoginUserSubject,
            TrnRequestApplicationUserId = trnRequestApplicationUserId,
            TrnRequestId = trnRequestId
        };

        createdEvent = new LegacyEvents.SupportTaskCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = now,
            RaisedBy = createdBy,
            SupportTask = EventModels.SupportTask.FromModel(task)
        };

        return task;
    }

    public T GetData<T>() where T : ISupportTaskData => (T)Data;

    public T UpdateData<T>(Func<T, T> update) where T : ISupportTaskData
    {
        var currentValue = GetData<T>();
        Data = update(currentValue);
        return (T)Data;
    }
}
