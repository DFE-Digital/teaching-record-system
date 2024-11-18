using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RedisRateLimiting;
using RedisRateLimiting.AspNetCore;
using StackExchange.Redis;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.Infrastructure.RateLimiting;

public static class ServiceCollectionExtensions
{
    private const string UnknownUserPartitionKey = "__UNKNOWN__";

    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        if (environment.IsProduction())
        {
            services.AddOptions<ClientIdRateLimiterOptions>()
                .Bind(configuration.GetSection("RateLimiting"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddRateLimiter(options =>
            {
                const string _windowSecondsHttpContextKey = "RateLimitWindowSeconds";

                options.OnRejected = async (context, token) =>
                {
                    await RateLimitMetadata.OnRejected(context.HttpContext, context.Lease, token);

                    if (context.Lease.TryGetMetadata(RateLimitMetadataName.Limit, out var limit) &&
                        context.HttpContext.Items.TryGetValue(_windowSecondsHttpContextKey, out var windowSeconds))
                    {
                        var message = $"A maximum of {limit} calls per {windowSeconds} seconds are permitted.";

                        var problemDetails = new ProblemDetails()
                        {
                            Title = "API calls quota exceeded",
                            Detail = message,
                            Status = context.HttpContext.Response.StatusCode
                        };

                        await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, type: typeof(ProblemDetails), options: null, contentType: "application/problem+json");
                    }
                };

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var rateLimiterOptions = httpContext.RequestServices.GetRequiredService<IOptions<ClientIdRateLimiterOptions>>().Value;
                    var connectionMultiplexerFactory = () => httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                    var partitionKey = ClaimsPrincipalCurrentUserProvider.TryGetCurrentApplicationUserFromHttpContext(httpContext, out var userId) ? userId.ToString() : UnknownUserPartitionKey;
                    var clientRateLimit = rateLimiterOptions.ClientRateLimits.TryGetValue(partitionKey, out var windowOptions) ? windowOptions : rateLimiterOptions.DefaultRateLimit;

                    // Window isn't available via RateLimitMetadata so stash it on the HttpContext instead
                    httpContext.Items.TryAdd(_windowSecondsHttpContextKey, clientRateLimit.Window.TotalSeconds);

                    return RedisRateLimitPartition.GetFixedWindowRateLimiter(partitionKey, key => new RedisFixedWindowRateLimiterOptions()
                    {
                        Window = clientRateLimit.Window,
                        PermitLimit = clientRateLimit.PermitLimit,
                        ConnectionMultiplexerFactory = connectionMultiplexerFactory,
                    });
                });
            });
        }

        return services;
    }
}
