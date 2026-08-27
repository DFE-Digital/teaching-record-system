namespace TeachingRecordSystem.Core.Events;

public record MandatoryQualificationMigratedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.MandatoryQualification MandatoryQualification { get; init; }
    public required MandatoryQualificationMigratedEventChanges Changes { get; init; }
}

[Flags]
public enum MandatoryQualificationMigratedEventChanges
{
    None = 0,
    Provider = 1 << 0,
    Specialism = 1 << 1
}
