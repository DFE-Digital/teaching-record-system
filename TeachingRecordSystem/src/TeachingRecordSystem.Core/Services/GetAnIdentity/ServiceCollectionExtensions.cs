using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api;

namespace TeachingRecordSystem.Core.Services.GetAnIdentityApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!environment.IsUnitTests() && !environment.IsEndToEndTests())
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
        }

        return services;
    }
}
