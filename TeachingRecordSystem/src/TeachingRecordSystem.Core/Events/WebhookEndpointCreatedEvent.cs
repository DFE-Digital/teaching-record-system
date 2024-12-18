using TeachingRecordSystem.Core.Events.Models;

namespace TeachingRecordSystem.Core.Events;

public record WebhookEndpointCreatedEvent : EventBase
{
    public required WebhookEndpoint WebhookEndpoint { get; init; }
}
