namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class User
{
    public required Guid UserId { get; init; }
    public required bool Active { get; set; }
    public required UserType UserType { get; init; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? AzureAdUserId { get; set; }
    public required string[] Roles { get; set; }
}
