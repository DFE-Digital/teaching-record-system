namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts;

public class RolesWithoutAlertWritePermissionDataAttribute : RoleNamesData
{
    public RolesWithoutAlertWritePermissionDataAttribute()
        : base(includeNoRoles: true, except: [UserRoles.AlertsManagerTra, UserRoles.AlertsManagerTraDbs, UserRoles.Administrator])
    {

    }
}
