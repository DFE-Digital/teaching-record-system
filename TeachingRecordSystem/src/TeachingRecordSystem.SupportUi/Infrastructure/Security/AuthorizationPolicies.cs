namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class AuthorizationPolicies
{
    public const string SupportTasksView = nameof(SupportTasksView);
    public const string SupportTasksEdit = nameof(SupportTasksEdit);
    public const string AdminOnly = nameof(AdminOnly);
    public const string UserManagement = nameof(UserManagement);
    public const string DbsAlertsFlag = nameof(DbsAlertsFlag);
    public const string DbsAlertsRead = nameof(DbsAlertsRead);
    public const string DbsAlertsWrite = nameof(DbsAlertsWrite);
    public const string NonDbsAlertsFlag = nameof(NonDbsAlertsFlag);
    public const string NonDbsAlertsRead = nameof(NonDbsAlertsRead);
    public const string NonDbsAlertsWrite = nameof(NonDbsAlertsWrite);
    public const string AlertsRead = nameof(AlertsRead);
    public const string AlertsWrite = nameof(AlertsWrite);
    public const string NonPersonOrAlertDataView = nameof(NonPersonOrAlertDataView);
    public const string NonPersonOrAlertDataEdit = nameof(NonPersonOrAlertDataEdit);
    public const string PersonDataEdit = nameof(PersonDataEdit);
    public const string NotesView = nameof(NotesView);
}
