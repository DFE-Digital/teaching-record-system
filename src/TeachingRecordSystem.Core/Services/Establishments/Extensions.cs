using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.Establishments.Gias;

namespace TeachingRecordSystem.Core.Services.Establishments;

public static class Extensions
{
    public static IServiceCollection AddGias(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<GiasOptions>()
            .Bind(configuration.GetSection("Gias"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddTransient<EstablishmentRefresher>()
            .AddSingleton<IEstablishmentMasterDataService, CsvDownloadEstablishmentMasterDataService>()
            .AddHttpClient<IEstablishmentMasterDataService, CsvDownloadEstablishmentMasterDataService>((sp, httpClient) =>
            {
                var options = sp.GetRequiredService<IOptions<GiasOptions>>();
                httpClient.BaseAddress = new Uri(options.Value.BaseDownloadAddress);
            });

        return services;
    }
}
