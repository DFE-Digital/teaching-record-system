using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record WebhookEndpointUpdatedEvent : EventBase
{
    public required WebhookEndpoint WebhookEndpoint { get; init; }
    public required WebhookEndpointUpdatedChanges Changes { get; init; }
}

[Flags]
public enum WebhookEndpointUpdatedChanges
{
    None = 0,
    Address = 1 << 0,
    ApiVersion = 1 << 1,
    CloudEventTypes = 1 << 2,
    Enabled = 1 << 3
}
