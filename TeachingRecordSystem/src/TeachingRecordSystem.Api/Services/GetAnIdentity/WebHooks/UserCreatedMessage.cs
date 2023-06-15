namespace TeachingRecordSystem.Api.Services.GetAnIdentity.WebHooks;

public record UserCreatedMessage : INotificationMessage
{
    public const string MessageTypeName = "UserCreated";

    public required User User { get; init; }
}
