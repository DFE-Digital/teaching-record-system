using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.Establishments.Gias;

namespace TeachingRecordSystem.Core.Services.Establishments;

public static class ServiceCollectionExtensions
{
    public static IHostApplicationBuilder AddGias(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            builder.Services.AddOptions<GiasOptions>()
                .Bind(builder.Configuration.GetSection("Gias"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddSingleton<EstablishmentRefresher>()
                .AddSingleton<IEstablishmentMasterDataService, CsvDownloadEstablishmentMasterDataService>()
                .AddHttpClient<IEstablishmentMasterDataService, CsvDownloadEstablishmentMasterDataService>((sp, httpClient) =>
                {
                    var options = sp.GetRequiredService<IOptions<GiasOptions>>();
                    httpClient.BaseAddress = new Uri(options.Value.BaseDownloadAddress);
                });
        }

        return builder;
    }
}
