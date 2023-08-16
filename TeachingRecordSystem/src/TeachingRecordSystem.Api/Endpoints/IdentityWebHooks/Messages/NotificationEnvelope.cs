namespace TeachingRecordSystem.Api.Endpoints.IdentityWebHooks.Messages;

public record NotificationEnvelope
{
    public required Guid NotificationId { get; init; }
    public required DateTime TimeUtc { get; init; }
    public required string MessageType { get; init; }
    public required INotificationMessage Message { get; init; }
}
