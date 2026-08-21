namespace TeachingRecordSystem.Core.Events;

public record WebhookEndpointDeletedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required EventModels.WebhookEndpoint WebhookEndpoint { get; init; }
}
