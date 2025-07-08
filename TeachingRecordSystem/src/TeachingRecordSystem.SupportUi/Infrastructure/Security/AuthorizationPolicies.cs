namespace TeachingRecordSystem.SupportUi.Infrastructure.Security;

public static class AuthorizationPolicies
{
    public const string SupportTasksView = nameof(SupportTasksView);
    public const string SupportTasksEdit = nameof(SupportTasksEdit);
    public const string PersonDataEdit = nameof(PersonDataEdit);
    public const string NonPersonOrAlertDataView = nameof(NonPersonOrAlertDataView);
    public const string NonPersonOrAlertDataEdit = nameof(NonPersonOrAlertDataEdit);
    public const string AlertsView = nameof(AlertsView);
    public const string AlertsEdit = nameof(AlertsEdit);
    public const string NotesView = nameof(NotesView);
    public const string UserManagement = nameof(UserManagement);
    public const string AdminOnly = nameof(AdminOnly);
}
