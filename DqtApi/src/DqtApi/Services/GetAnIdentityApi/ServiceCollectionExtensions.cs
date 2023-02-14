using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DqtApi.Services.GetAnIdentityApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (!environment.IsUnitTests() && !environment.IsEndToEndTests())
        {
            services.AddOptions<GetAnIdentityApiOptions>()
                .Bind(configuration.GetSection("GetAnIdentity"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services
                .AddTransient<ClientCredentialsBearerTokenDelegatingHandler>()
                .AddHttpClient<IGetAnIdentityApiClient, GetAnIdentityApiClient>((sp, httpClient) =>
                {
                    var options = sp.GetRequiredService<IOptions<GetAnIdentityApiOptions>>();
                    httpClient.BaseAddress = new Uri(options.Value.BaseAddress);
                })
                .AddHttpMessageHandler<ClientCredentialsBearerTokenDelegatingHandler>(); ;
        }
        return services;
    }
}
