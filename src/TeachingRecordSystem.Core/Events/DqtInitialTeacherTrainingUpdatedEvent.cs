namespace TeachingRecordSystem.Core.Events;

public record DqtInitialTeacherTrainingUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.DqtInitialTeacherTraining? InitialTeacherTraining { get; init; }
    public required EventModels.DqtInitialTeacherTraining? OldInitialTeacherTraining { get; init; }
    public required DqtInitialTeacherTrainingUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum DqtInitialTeacherTrainingUpdatedEventChanges
{
    None = 0,
    Result = 1 << 0
}
