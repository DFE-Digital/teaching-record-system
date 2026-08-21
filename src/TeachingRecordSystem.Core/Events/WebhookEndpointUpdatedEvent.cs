namespace TeachingRecordSystem.Core.Events;

public record WebhookEndpointUpdatedEvent : IEvent
{
    public required Guid EventId { get; init; }
    Guid[] IEvent.PersonIds => [];
    string[] IEvent.OneLoginUserSubjects => [];
    string[] IEvent.SupportTaskReferences => [];
    public required EventModels.WebhookEndpoint WebhookEndpoint { get; init; }
    public required WebhookEndpointUpdatedEventChanges Changes { get; init; }
}

[Flags]
public enum WebhookEndpointUpdatedEventChanges
{
    None = 0,
    Address = 1 << 0,
    ApiVersion = 1 << 1,
    CloudEventTypes = 1 << 2,
    Enabled = 1 << 3
}
