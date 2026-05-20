using Microsoft.AspNetCore.Authorization;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Security.Requirements;

public record SupportTasksEditRequirement : IAuthorizationRequirement;
