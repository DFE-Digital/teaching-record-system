using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DqtApi.Middleware
{
    public class AppendSecurityResponseHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public AppendSecurityResponseHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext context)
        {
            var response = context.Response;

            if (!response.HasStarted)
            {
                response.Headers["X-Frame-Options"] = "DENY";
                response.Headers["X-Xss-Protection"] = "1; mode=block";
                response.Headers["X-Content-Type-Options"] = "nosniff";
                response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
                response.Headers["Referrer-Policy"] = "no-referrer";
                response.Headers["Content-Security-Policy"] = "default-src 'self'";
            }

            return _next(context);
        }
    }
}
