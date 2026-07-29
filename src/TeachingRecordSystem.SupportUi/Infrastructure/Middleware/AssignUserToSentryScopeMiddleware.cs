using System.Security.Claims;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Middleware;

public class AssignUserToSentryScopeMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var userId = context.User.FindFirstValue(CustomClaims.UserId);

        if (userId is not null)
        {
            SentrySdk.ConfigureScope(scope => scope.User = new SentryUser { Id = userId });
        }

        return next(context);
    }
}
