namespace TeachingRecordSystem.Api.Endpoints.IdentityWebHooks.Messages;

public record UserCreatedMessage : INotificationMessage
{
    public const string MessageTypeName = "UserCreated";

    public required User User { get; init; }
}
