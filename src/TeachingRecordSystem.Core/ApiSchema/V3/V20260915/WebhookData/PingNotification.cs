namespace TeachingRecordSystem.Core.ApiSchema.V3.V20260915.WebhookData;

public record PingNotification : IWebhookMessageData
{
    public static string CloudEventType { get; } = "ping";

    public required Guid PingId { get; init; }
}
