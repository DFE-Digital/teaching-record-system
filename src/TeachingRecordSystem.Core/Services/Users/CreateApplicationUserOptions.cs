namespace TeachingRecordSystem.Core.Services.Users;

public record CreateApplicationUserOptions
{
    public required string Name { get; init; }
    public required string? ShortName { get; init; }
}
