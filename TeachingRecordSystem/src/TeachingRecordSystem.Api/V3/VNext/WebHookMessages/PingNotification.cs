namespace TeachingRecordSystem.Api.V3.VNext.WebHookMessages;

public record PingNotification
{
    public required Guid PingId { get; init; }
}
