namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.WebhookPayloads;

public record PingNotification
{
    public required Guid PingId { get; init; }
}
