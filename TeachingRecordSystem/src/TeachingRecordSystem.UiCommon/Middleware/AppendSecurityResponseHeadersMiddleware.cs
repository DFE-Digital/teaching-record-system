using Microsoft.AspNetCore.Http;

namespace TeachingRecordSystem.UiCommon.Middleware;

public class AppendSecurityResponseHeadersMiddleware(RequestDelegate next)
{
    public Task Invoke(HttpContext context)
    {
        var response = context.Response;

        response.Headers["X-Frame-Options"] = "DENY";
        response.Headers["X-Xss-Protection"] = "0";
        response.Headers["X-Content-Type-Options"] = "nosniff";
        response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
        response.Headers["Referrer-Policy"] = "no-referrer-when-downgrade";

        return next(context);
    }
}
