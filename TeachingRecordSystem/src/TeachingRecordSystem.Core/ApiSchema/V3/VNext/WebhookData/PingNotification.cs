namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.WebhookData;

public record PingNotification
{
    public required Guid PingId { get; init; }
}
