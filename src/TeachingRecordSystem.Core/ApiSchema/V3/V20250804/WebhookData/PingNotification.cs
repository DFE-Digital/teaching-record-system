namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250804.WebhookData;

public record PingNotification
{
    public required Guid PingId { get; init; }
}
