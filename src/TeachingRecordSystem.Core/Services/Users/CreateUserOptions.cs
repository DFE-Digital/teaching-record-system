namespace TeachingRecordSystem.Core.Services.Users;

public record CreateUserOptions
{
    public required string Name { get; init; }
    public required string? Email { get; init; }
    public required string? AzureAdUserId { get; init; }
    public required string? Role { get; init; }
}
