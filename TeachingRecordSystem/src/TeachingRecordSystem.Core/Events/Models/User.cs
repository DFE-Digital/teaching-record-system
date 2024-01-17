namespace TeachingRecordSystem.Core.Events.Models;

public record User
{
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public string? Email { get; init; }
    public string? AzureAdUserId { get; init; }
    public required string[] Roles { get; init; }

    public static User FromModel(DataStore.Postgres.Models.User user) => new()
    {
        UserId = user.UserId,
        Name = user.Name,
        Email = user.Email,
        AzureAdUserId = user.AzureAdUserId,
        Roles = user.Roles,
    };
}
