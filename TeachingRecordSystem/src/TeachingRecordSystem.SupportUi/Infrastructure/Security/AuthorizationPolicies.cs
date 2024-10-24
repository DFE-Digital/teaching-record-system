namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class AuthorizationPolicies
{
    public const string ChangeRequestManagement = "ChangeRequestManagement";
    public const string Hangfire = "Hangfire";
    public const string UserManagement = "UserManagement";
    public const string DbsAlertFlag = "DbsAlertFlag";
    public const string DbsAlertRead = "DbsAlertRead";
    public const string DbsAlertWrite = "DbsAlertWrite";
    public const string NonDbsAlertFlag = "NonDbsAlertFlag";
    public const string NonDbsAlertRead = "NonDbsAlertRead";
    public const string NonDbsAlertWrite = "NonDbsAlertWrite";
}
