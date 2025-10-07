using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events.Legacy;

public record WebhookEndpointCreatedEvent : EventBase
{
    public required WebhookEndpoint WebhookEndpoint { get; init; }
}
