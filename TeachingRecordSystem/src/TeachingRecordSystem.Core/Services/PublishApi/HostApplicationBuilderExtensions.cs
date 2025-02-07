using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.Core.Services.PublishApi;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddPublishApi(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            builder.Services.AddOptions<PublishApiOptions>()
                .Bind(builder.Configuration.GetSection("PublishApi"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddHttpClient<IPublishApiClient, PublishApiClient>((sp, httpClient) =>
                {
                    var options = sp.GetRequiredService<IOptions<PublishApiOptions>>();
                    httpClient.BaseAddress = new Uri(options.Value.BaseAddress);
                });
        }

        return builder;
    }
}
