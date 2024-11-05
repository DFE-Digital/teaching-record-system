using System.Text.Json;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class WebhookMessage
{
    public required Guid WebhookMessageId { get; init; }
    public required Guid WebhookEndpointId { get; init; }
    public virtual WebhookEndpoint WebhookEndpoint { get; } = null!;
    public required string CloudEventId { get; init; }
    public required string CloudEventType { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required string ApiVersion { get; init; }
    public required JsonElement Data { get; init; }
    public required DateTime? NextDeliveryAttempt { get; set; }
    public DateTime? Delivered { get; set; }
    public List<DateTime> DeliveryAttempts { get; set; } = [];
    public List<string> DeliveryErrors { get; set; } = [];
}
