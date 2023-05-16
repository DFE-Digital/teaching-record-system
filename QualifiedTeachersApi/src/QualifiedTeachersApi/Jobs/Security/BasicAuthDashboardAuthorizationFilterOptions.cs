namespace QualifiedTeachersApi.Jobs.Security;

public class BasicAuthDashboardAuthorizationFilterOptions
{   
    public bool LoginCaseSensitive { get; set; }
    
    public string Username { get; set; }

    public string Password { get; set; }
}
