using Microsoft.AspNetCore.Authorization;
using static TeachingRecordSystem.SupportUi.Infrastructure.Security.Permissions;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

public class AlertRequirement : IAuthorizationRequirement
{
    public Alerts AlertsPermission { get; }

    public AlertRequirement(Alerts alertsPermission)
    {
        AlertsPermission = alertsPermission;
    }
}
