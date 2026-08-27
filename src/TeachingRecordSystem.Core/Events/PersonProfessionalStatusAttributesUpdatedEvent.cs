namespace TeachingRecordSystem.Core.Events;

public record PersonProfessionalStatusAttributesUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.ProfessionalStatusPersonAttributes PersonAttributes { get; init; }
    public required EventModels.ProfessionalStatusPersonAttributes OldPersonAttributes { get; init; }
    public required PersonProfessionalStatusAttributesUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum PersonProfessionalStatusAttributesUpdatedEventChanges
{
    None = 0,
    QtsDate = 1 << 0,
    EytsDate = 1 << 1,
    HasEyps = 1 << 2,
    PqtsDate = 1 << 3,
    QtlsStatus = 1 << 4
}
