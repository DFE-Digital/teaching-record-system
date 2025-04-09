using Microsoft.AspNetCore.Authorization;
using static TeachingRecordSystem.SupportUi.Infrastructure.Security.Permissions;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

public class DbsAlertRequirement : IAuthorizationRequirement
{
    public Alerts AlertsPermission { get; }

    public DbsAlertRequirement(Alerts alertsPermission)
    {
        AlertsPermission = alertsPermission;
    }
}
