#nullable disable
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace QualifiedTeachersApi.Logging;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app, bool logRequestBody)
    {
        app.Use(async (ctx, next) =>
        {
            using (LogContext.Push(new RemoveRedactedUrlParametersEnricher(ctx)))
            using (LogContext.PushProperty("CorrelationId", ctx.TraceIdentifier))
            {
                if (logRequestBody)
                {
                    if (ctx.Request.GetTypedHeaders().ContentType?.MediaType == "application/json")
                    {
                        ctx.Request.EnableBuffering();

                        const int bufferSize = 1024;

                        using (var reader = new StreamReader(
                                   ctx.Request.Body,
                                   encoding: Encoding.UTF8,
                                   detectEncodingFromByteOrderMarks: false,
                                   bufferSize: bufferSize,
                                   leaveOpen: true))
                        {
                            var body = await reader.ReadToEndAsync();
                            LogContext.PushProperty("RequestBody", body);

                            ctx.Request.Body.Seek(0L, SeekOrigin.Begin);
                        }
                    }
                }

                await next();
            }
        });

        app.UseSerilogRequestLogging();

        return app;
    }
}
