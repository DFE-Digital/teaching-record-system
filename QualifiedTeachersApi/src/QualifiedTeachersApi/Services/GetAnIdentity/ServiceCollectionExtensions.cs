#nullable disable
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace QualifiedTeachersApi.Services.GetAnIdentityApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityApi(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        if (!environment.IsUnitTests() && !environment.IsEndToEndTests())
        {
            services.AddOptions<GetAnIdentityOptions>()
                .Bind(configuration.GetSection("GetAnIdentity"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddTransient<IHostedService, LinkTrnToIdentityUserService>();

            services
                .AddTransient<ClientCredentialsBearerTokenDelegatingHandler>()
                .AddHttpClient<IGetAnIdentityApiClient, GetAnIdentityApiClient>((sp, httpClient) =>
                {
                    var options = sp.GetRequiredService<IOptions<GetAnIdentityOptions>>();
                    httpClient.BaseAddress = new Uri(options.Value.BaseAddress);
                })
                .AddHttpMessageHandler<ClientCredentialsBearerTokenDelegatingHandler>(); ;
        }

        return services;
    }
}
