namespace QualifiedTeachersApi.Jobs.Security;

public class BasicAuthDashboardAuthorizationFilterOptions
{
    public bool LoginCaseSensitive { get; set; }

    public required string Username { get; set; }

    public required string Password { get; set; }
}
