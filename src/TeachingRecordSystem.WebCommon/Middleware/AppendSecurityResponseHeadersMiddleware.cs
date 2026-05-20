using Microsoft.AspNetCore.Http;

namespace TeachingRecordSystem.WebCommon.Middleware;

public class AppendSecurityResponseHeadersMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(HttpContext context)
    {
        var response = context.Response;

        response.Headers["X-Frame-Options"] = "SAMEORIGIN";
        response.Headers["X-Xss-Protection"] = "0";
        response.Headers["X-Content-Type-Options"] = "nosniff";
        response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
        response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

        return next(context);
    }
}
