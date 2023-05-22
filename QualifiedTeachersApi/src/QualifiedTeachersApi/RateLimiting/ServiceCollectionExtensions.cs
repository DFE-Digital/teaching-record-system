using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using QualifiedTeachersApi.Infrastructure.Security;
using RedisRateLimiting;
using RedisRateLimiting.AspNetCore;
using StackExchange.Redis;

namespace QualifiedTeachersApi.RateLimiting;

public static class ServiceCollectionExtensions
{
    private const string UnknownClientId = "__UNKNOWN__";

    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        if (environment.IsProduction())
        {
            services.Configure<ClientIdRateLimiterOptions>(configuration.GetSection("RateLimiting"));
            services.AddRateLimiter(options =>
            {
                options.OnRejected = async (context, token) =>
                {
                    // The default OnRejected handler sets the status code and adds some useful headers so we would also like that in our policy OnRejected handler which overrides it
                    await RateLimitMetadata.OnRejected.Invoke(context.HttpContext, context.Lease, token);

                    var rateLimitOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<ClientIdRateLimiterOptions>>();
                    var clientId = ClaimsPrincipalCurrentClientProvider.GetCurrentClientIdFromHttpContext(context.HttpContext) ?? UnknownClientId;
                    var clientRateLimit = GetFixedWindowRateLimiterOptionsForClient(clientId, rateLimitOptions.Value);
                    var message = string.Format("A maximum of {0} calls per {1} seconds are permitted.", clientRateLimit.PermitLimit, clientRateLimit.Window.TotalSeconds);

                    var problemDetails = new ProblemDetails()
                    {
                        Title = "API calls quota exceeded",
                        Detail = message,
                        Status = context.HttpContext.Response.StatusCode
                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, type: typeof(ProblemDetails), options: null, contentType: "application/problem+json");
                };

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var rateLimitOptions = httpContext.RequestServices.GetRequiredService<IOptions<ClientIdRateLimiterOptions>>();
                    var connectionMultiplexerFactory = () => httpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                    var clientId = ClaimsPrincipalCurrentClientProvider.GetCurrentClientIdFromHttpContext(httpContext) ?? UnknownClientId;
                    var clientRateLimit = GetFixedWindowRateLimiterOptionsForClient(clientId, rateLimitOptions.Value);

                    return RedisRateLimitPartition.GetFixedWindowRateLimiter(clientId, key => new RedisFixedWindowRateLimiterOptions
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

    private static FixedWindowOptions GetFixedWindowRateLimiterOptionsForClient(string clientId, ClientIdRateLimiterOptions rateLimiterOptions)
    {
        FixedWindowOptions? clientRateLimit;
        if (rateLimiterOptions.ClientRateLimits is null || !rateLimiterOptions.ClientRateLimits.TryGetValue(clientId, out clientRateLimit))
        {
            clientRateLimit = rateLimiterOptions.DefaultRateLimit;
        }

        return clientRateLimit;
    }
}
