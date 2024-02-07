using Serilog.Context;
using TeachingRecordSystem.Api.Infrastructure.Features;

namespace TeachingRecordSystem.Api.Infrastructure.Middleware;

public class AssignRequestedVersionMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        string? requestedMinorVersion = context.Request.Headers.TryGetValue(VersionRegistry.MinorVersionHeaderName, out var requestedVersionValues) ?
            requestedVersionValues.ToString() :
            null;

        var feature = new RequestedVersionFeature(requestedMinorVersion);
        context.Features.Set(feature);

        LogContext.PushProperty("RequestedMinorVersion", feature.RequestedMinorVersion);
        LogContext.PushProperty("EffectiveMinorVersion", feature.EffectiveMinorVersion);

        await next.Invoke(context);
    }
}
