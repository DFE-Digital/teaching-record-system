using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.Core.Services.TrnGeneration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTrnGeneration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<TrnGenerationApiOptions>()
            .Bind(configuration.GetSection("TrnGenerationApi"))
            .ValidateDataAnnotations();

        services
            .AddSingleton<ITrnGenerator, ApiTrnGenerator>()
            .AddHttpClient<ITrnGenerator, ApiTrnGenerator>((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<TrnGenerationApiOptions>>();
                httpClient.BaseAddress = new Uri(options.Value.BaseAddress);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.Value.ApiKey);
                httpClient.Timeout = TimeSpan.FromSeconds(5);
            });

        return services;
    }
}
