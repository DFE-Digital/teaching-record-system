using Dfe.Analytics.EFCore.AirbyteApi;
using Dfe.Analytics.EFCore.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ConfigurationProvider = Dfe.Analytics.EFCore.Configuration.ConfigurationProvider;

namespace Dfe.Analytics.EFCore;

public static class Extensions
{
    public static IHostApplicationBuilder AddDfeAnalyticsDeploymentTools(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddDfeAnalyticsDeploymentTools();

        builder.Services.Configure<AirbyteOptions>(options =>
        {
            builder.Configuration.GetSection("DfeAnalytics:Airbyte").Bind(options);
        });

        return builder;
    }

    public static IServiceCollection AddDfeAnalyticsDeploymentTools(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<AirbyteOptions>()
            .ValidateOnStart();

        services
            .AddSingleton<AirbyteApiClient>()
            .AddSingleton<ConfigurationProvider>()
            .AddSingleton<AnalyticsDeployer>();

        var airbyteApiClientBuilder = services.AddHttpClient<AirbyteApiClient>();
        AirbyteApiClient.ConfigureHttpClient(airbyteApiClientBuilder);

        return services;
    }

    public static IServiceCollection ConfigureAirbyteOptions(this IServiceCollection services, Action<AirbyteOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);

        return services;
    }
}
