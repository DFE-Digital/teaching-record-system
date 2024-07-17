namespace TeachingRecordSystem.Api.Webhooks;

public record WebhookEnvelope
{
    public required string CloudEventId { get; init; }
    public required string CloudEventType { get; init; }
    public required string MinorVersion { get; init; }
    public required DateTimeOffset TimestampUtc { get; init; }
    public required object Data { get; init; }
}
