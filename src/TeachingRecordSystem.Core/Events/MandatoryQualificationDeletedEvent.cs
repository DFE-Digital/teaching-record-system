namespace TeachingRecordSystem.Core.Events;

public record MandatoryQualificationDeletedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.MandatoryQualification MandatoryQualification { get; init; }
}
