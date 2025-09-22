using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.Core.Services.PublishApi;

public static class Extensions
{
    public static IServiceCollection AddPublishApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PublishApiOptions>()
            .Bind(configuration.GetSection("PublishApi"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddHttpClient<IPublishApiClient, PublishApiClient>((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<PublishApiOptions>>();
                httpClient.BaseAddress = new Uri(options.Value.BaseAddress);
            });

        return services;
    }
}
