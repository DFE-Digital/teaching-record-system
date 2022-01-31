using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Context;

namespace DqtApi.Logging
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        {
            app.Use((ctx, next) =>
            {
                LogContext.Push(new RemoveRedactedUrlParametersEnricher(ctx));
                LogContext.PushProperty("CorrelationId", ctx.TraceIdentifier);
                return next();
            });

            app.UseSerilogRequestLogging();

            return app;
        }
    }
}
