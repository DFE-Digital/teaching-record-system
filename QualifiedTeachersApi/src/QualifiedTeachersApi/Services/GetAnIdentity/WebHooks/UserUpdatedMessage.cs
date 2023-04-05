#nullable disable
namespace QualifiedTeachersApi.Services.GetAnIdentity.WebHooks;

public record UserUpdatedMessage
{
    public const string MessageTypeName = "UserUpdated";

    public required User User { get; init; }
}
