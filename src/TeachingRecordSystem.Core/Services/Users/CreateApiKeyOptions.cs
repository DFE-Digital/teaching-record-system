namespace TeachingRecordSystem.Core.Services.Users;

public record CreateApiKeyOptions
{
    public required Guid ApplicationUserId { get; init; }
    public required string Key { get; init; }
}
