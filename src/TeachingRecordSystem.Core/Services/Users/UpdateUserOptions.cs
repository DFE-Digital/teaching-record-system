namespace TeachingRecordSystem.Core.Services.Users;

public record UpdateUserOptions
{
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required string? Role { get; init; }
}
