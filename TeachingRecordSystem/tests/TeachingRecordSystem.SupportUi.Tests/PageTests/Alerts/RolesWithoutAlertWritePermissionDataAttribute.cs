namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts;

public class RolesWithoutAlertWritePermissionDataAttribute() : RoleNamesData(includeNoRoles: true,
    except: [UserRoles.AlertsManagerTra, UserRoles.AlertsManagerTraDbs, UserRoles.Administrator]);
