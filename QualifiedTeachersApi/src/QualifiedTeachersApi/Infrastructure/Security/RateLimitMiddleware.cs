#nullable disable
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace QualifiedTeachersApi.Infrastructure.Security;

public class RateLimitMiddleware : ClientRateLimitMiddleware
{
    private readonly ClientRateLimitOptions _options;

    public RateLimitMiddleware(
        RequestDelegate next,
        IProcessingStrategy processingStrategy,
        IOptions<ClientRateLimitOptions> options,
        IClientPolicyStore policyStore,
        IRateLimitConfiguration config,
        ILogger<ClientRateLimitMiddleware> logger)
        : base(next, processingStrategy, options, policyStore, config, logger)
    {
        _options = options.Value;
    }

    public override Task ReturnQuotaExceededResponse(HttpContext httpContext, RateLimitRule rule, string retryAfter)
    {
        if (rule.QuotaExceededResponse != null)
        {
            _options.QuotaExceededResponse = rule.QuotaExceededResponse;
        }

        var message = string.Format(
            _options.QuotaExceededResponse?.Content ??
            _options.QuotaExceededMessage ??
            "A maximum of {0} calls per {1} are permitted.", rule.Limit, rule.Period, retryAfter);

        if (!_options.DisableRateLimitHeaders)
        {
            httpContext.Response.Headers["Retry-After"] = retryAfter;
        }

        var statusCode = _options.QuotaExceededResponse?.StatusCode ?? _options.HttpStatusCode;

        var problemDetails = new ProblemDetails()
        {
            Title = "API calls quota exceeded",
            Detail = message,
            Status = statusCode
        };

        httpContext.Response.StatusCode = statusCode;

        return httpContext.Response.WriteAsJsonAsync(problemDetails, type: typeof(ProblemDetails), options: null, contentType: "application/problem+json");
    }
}
