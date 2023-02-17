using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;

namespace QualifiedTeachersApi.Services.TrnGenerationApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTrnGenerationApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<TrnGenerationApiOptions>()
                .Bind(configuration.GetSection("TrnGenerationApi"))
                .ValidateDataAnnotations();

        services
            .AddHttpClient<ITrnGenerationApiClient, TrnGenerationApiClient>((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<TrnGenerationApiOptions>>();
                httpClient.BaseAddress = new Uri(options.Value.BaseAddress);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
            });

        services
            .AddSingleton<ITrnGenerationApiClient>(sp =>
            {
                var featureManager = sp.GetRequiredService<IFeatureManager>();
                var isUseTrnGenerationApiEnabled = featureManager.IsEnabledAsync(FeatureFlags.UseTrnGenerationApi).GetAwaiter().GetResult();
                if (isUseTrnGenerationApiEnabled)
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient(nameof(ITrnGenerationApiClient));
                    return new TrnGenerationApiClient(httpClient);
                }
                else
                {
                    return new NoopTrnGenerationApiClient();
                }
            });

        return services;
    }
}
