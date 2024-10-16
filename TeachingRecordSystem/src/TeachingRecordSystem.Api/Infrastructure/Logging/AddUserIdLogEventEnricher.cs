using System.Security.Claims;
using Serilog.Core;
using Serilog.Events;

namespace TeachingRecordSystem.Api.Infrastructure.Logging;

public class AddUserIdLogEventEnricher(IHttpContextAccessor httpContextAccessor) : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No HttpContext.");
        var principal = httpContext.User;

        if (principal?.Identity is null || !principal.Identity.IsAuthenticated)
        {
            return;
        }

        var applicationUserId = principal.FindFirstValue("sub");
        var applicationUserName = principal.FindFirstValue(ClaimTypes.Name);

        logEvent.AddOrUpdateProperty(new LogEventProperty("ApplicationUserId", new ScalarValue(applicationUserId)));
        logEvent.AddOrUpdateProperty(new LogEventProperty("ApplicationUserName", new ScalarValue(applicationUserName)));
    }
}
