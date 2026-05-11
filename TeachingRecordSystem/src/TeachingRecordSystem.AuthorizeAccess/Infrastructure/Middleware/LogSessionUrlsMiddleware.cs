using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Middleware;

public class LogSessionUrlsMiddleware(RequestDelegate next)
{
    private static readonly IReadOnlyCollection<string> _requestHeadersToLog = new HashSet<string>(["Cookie", "User-Agent"], StringComparer.OrdinalIgnoreCase);
    private static readonly IReadOnlyCollection<string> _responseHeadersToLog = new HashSet<string>(["Set-Cookie"], StringComparer.OrdinalIgnoreCase);

    public Task InvokeAsync(
        HttpContext context,
        TrsDbContext dbContext,
        IJourneyInstanceProvider journeyInstanceProvider,
        TimeProvider timeProvider)
    {
        context.Response.OnStarting(async () =>
        {
            if (context.Features.Get<ISessionFeature>() is not null &&
                context.Request.Path != "/health" &&
                context.GetEndpoint() is not null)
            {
                var coordinator = journeyInstanceProvider.GetSignInJourneyCoordinator(context);

                var sessionId = context.Session.Id;
                var timestamp = timeProvider.UtcNow;
                var method = context.Request.Method;
                var url = context.Request.GetEncodedPathAndQuery();
                var state = JsonSerializer.Serialize(coordinator?.State);

                if (context.Features.Get<IStatusCodeReExecuteFeature>() is { } statusCodeReExecuteFeature)
                {
                    url = statusCodeReExecuteFeature.OriginalPath + statusCodeReExecuteFeature.OriginalQueryString;
                }
                else if (context.Features.Get<IExceptionHandlerFeature>() is { } exceptionHandlerFeature)
                {
                    url = exceptionHandlerFeature.Path + context.Request.QueryString;
                }

                var requestHeaders = string.Join(
                    Environment.NewLine,
                    context.Request.Headers
                        .Where(h => _requestHeadersToLog.Contains(h.Key))
                        .Select(x => x.Key + ": " + x.Value));

                var responseHeaders = string.Join(
                    Environment.NewLine,
                    context.Response.Headers
                        .Where(h => _responseHeadersToLog.Contains(h.Key))
                        .Select(x => x.Key + ": " + x.Value));

                await dbContext.Database.ExecuteSqlAsync(
                    $"""
                         INSERT INTO session_urls (session_id, timestamp, method, url, state, request_headers, response_headers)
                         VALUES ({sessionId}, {timestamp}, {method}, {url}, {state}::jsonb, {requestHeaders}, {responseHeaders});
                     """);

                SentrySdk.SetTag("SessionId", sessionId);
            }
        });

        return next(context);
    }
}
