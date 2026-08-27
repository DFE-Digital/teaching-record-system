namespace TeachingRecordSystem.Core.Events;

public record RouteToProfessionalStatusCreatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [PersonId];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required Guid PersonId { get; init; }
    public required EventModels.RouteToProfessionalStatus RouteToProfessionalStatus { get; init; }
}
