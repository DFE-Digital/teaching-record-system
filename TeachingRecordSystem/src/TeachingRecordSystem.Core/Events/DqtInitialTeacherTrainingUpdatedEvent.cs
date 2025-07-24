using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record DqtInitialTeacherTrainingUpdatedEvent : EventBase, IEventWithPersonId
{
    public required Guid PersonId { get; init; }
    public required DqtInitialTeacherTraining? InitialTeacherTraining { get; init; }
    public required DqtInitialTeacherTraining? OldInitialTeacherTraining { get; init; }
    public required DqtInitialTeacherTrainingUpdatedEventChanges Changes { get; init; }
}

public enum DqtInitialTeacherTrainingUpdatedEventChanges
{
    None = 0,
    Result = 1 << 0
}
