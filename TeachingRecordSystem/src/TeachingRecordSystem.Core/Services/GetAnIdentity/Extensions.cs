using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api;
using TeachingRecordSystem.Core.Services.GetAnIdentityApi;

namespace TeachingRecordSystem.Core.Services.GetAnIdentity;

public static class Extensions
{
    public static IServiceCollection AddIdentityApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GetAnIdentityOptions>()
            .Bind(configuration.GetSection("GetAnIdentity"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddTransient<ClientCredentialsBearerTokenDelegatingHandler>()
            .AddHttpClient<ClientCredentialsBearerTokenDelegatingHandler>();

        services
            .AddHttpClient<IGetAnIdentityApiClient, GetAnIdentityApiClient>((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<GetAnIdentityOptions>>();
                httpClient.BaseAddress = new Uri(options.Value.BaseAddress);
            })
            .AddHttpMessageHandler<ClientCredentialsBearerTokenDelegatingHandler>();

        return services;
    }
}
