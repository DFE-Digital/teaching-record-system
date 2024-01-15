namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public abstract class UserBase
{
    public const int NameMaxLength = 200;

    public static Guid SystemUserId { get; } = new Guid("a81394d1-a498-46d8-af3e-e077596ab303");

    public required Guid UserId { get; init; }
    public required bool Active { get; set; }
    public UserType UserType { get; }
    public required string Name { get; set; }
}

public class User : UserBase
{
    public required string? Email { get; set; }
    public string? AzureAdUserId { get; set; }
    public required string[] Roles { get; set; }
    public Guid? DqtUserId { get; set; }
}

public class ApplicationUser : UserBase
{
    public required string[] ApiRoles { get; set; }
}

public class SystemUser : UserBase
{
}
