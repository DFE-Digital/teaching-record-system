namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class User
{
    public const int NameMaxLength = 200;

    public static Guid SystemUserId { get; } = new Guid("a81394d1-a498-46d8-af3e-e077596ab303");

    public required Guid UserId { get; init; }
    public required bool Active { get; set; }
    public required UserType UserType { get; init; }
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? AzureAdUserId { get; set; }
    public required string[] Roles { get; set; }
}
