namespace TeachingRecordSystem.Api.Endpoints.IdentityWebHooks.Messages;

public class UserMergedMessage : INotificationMessage
{
    public const string MessageTypeName = "UserMerged";

    public required Guid MergedUserId { get; init; }
    public required Guid MasterUserId { get; init; }
}
