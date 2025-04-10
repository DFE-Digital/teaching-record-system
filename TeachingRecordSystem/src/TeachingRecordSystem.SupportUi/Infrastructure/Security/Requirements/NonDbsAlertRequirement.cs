using Microsoft.AspNetCore.Authorization;
using static TeachingRecordSystem.SupportUi.Infrastructure.Security.Permissions;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

public class NonDbsAlertRequirement : IAuthorizationRequirement
{
    public Alerts AlertsPermission { get; }

    public NonDbsAlertRequirement(Alerts alertsPermission)
    {
        AlertsPermission = alertsPermission;
    }
}
