namespace TeachingRecordSystem.Core.Events;

public record DqtQtsRegistrationUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.DqtQtsRegistration? QtsRegistration { get; init; }
    public required EventModels.DqtQtsRegistration? OldQtsRegistration { get; init; }
    public required DqtQtsRegistrationUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum DqtQtsRegistrationUpdatedEventChanges
{
    None = 0,
    TeacherStatusValue = 1 << 0,
    EarlyYearsStatusValue = 1 << 1,
    QtsDate = 1 << 2,
    EytsDate = 1 << 3
}
