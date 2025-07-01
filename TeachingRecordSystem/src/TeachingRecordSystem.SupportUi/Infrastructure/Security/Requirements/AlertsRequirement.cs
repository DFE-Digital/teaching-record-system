using Microsoft.AspNetCore.Authorization;
using static TeachingRecordSystem.SupportUi.Infrastructure.Security.Permissions;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

public record AlertsRequirement(Alerts AlertsPermission) : IAuthorizationRequirement;
