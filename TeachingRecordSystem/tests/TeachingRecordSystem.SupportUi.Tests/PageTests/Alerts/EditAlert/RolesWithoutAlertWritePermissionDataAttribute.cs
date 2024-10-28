namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert;

public class RolesWithoutAlertWritePermissionDataAttribute : RoleNamesData
{
    public RolesWithoutAlertWritePermissionDataAttribute()
        : base(includeNoRoles: true, except: [UserRoles.AlertsReadWrite, UserRoles.DbsAlertsReadWrite, UserRoles.Administrator])
    {

    }
}
