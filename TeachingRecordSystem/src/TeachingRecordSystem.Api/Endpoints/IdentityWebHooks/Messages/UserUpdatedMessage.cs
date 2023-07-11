using Optional;

namespace TeachingRecordSystem.Api.Endpoints.IdentityWebHooks.Messages;

public record UserUpdatedMessage : INotificationMessage
{
    public const string MessageTypeName = "UserUpdated";

    public required User User { get; init; }
    public required UserUpdatedMessageChanges Changes { get; init; }
}

public record UserUpdatedMessageChanges
{
    public required Option<string?> Trn { get; init; }
}
