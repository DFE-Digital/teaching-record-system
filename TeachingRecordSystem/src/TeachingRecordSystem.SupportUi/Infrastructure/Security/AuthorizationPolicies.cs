namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class AuthorizationPolicies
{
    public const string ChangeRequestManagement = nameof(ChangeRequestManagement);
    public const string AdminOnly = nameof(AdminOnly);
    public const string UserManagement = nameof(UserManagement);
    public const string DbsAlertFlag = nameof(DbsAlertFlag);
    public const string DbsAlertRead = nameof(DbsAlertRead);
    public const string DbsAlertWrite = nameof(DbsAlertWrite);
    public const string NonDbsAlertFlag = nameof(NonDbsAlertFlag);
    public const string NonDbsAlertRead = nameof(NonDbsAlertRead);
    public const string NonDbsAlertWrite = nameof(NonDbsAlertWrite);
    public const string AlertWrite = nameof(AlertWrite);
    public const string InductionReadWrite = nameof(InductionReadWrite);
    public const string PersonDataEdit = nameof(PersonDataEdit);
    public const string RoutesView = nameof(RoutesView);
    public const string RoutesEdit = nameof(RoutesEdit);
}
