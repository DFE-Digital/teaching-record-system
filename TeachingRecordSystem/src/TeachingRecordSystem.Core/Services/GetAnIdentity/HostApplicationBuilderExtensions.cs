using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api;

namespace TeachingRecordSystem.Core.Services.GetAnIdentityApi;

public static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddIdentityApi(this IHostApplicationBuilder builder)
    {
        if (!builder.Environment.IsUnitTests() && !builder.Environment.IsEndToEndTests())
        {
            builder.Services.AddOptions<GetAnIdentityOptions>()
                .Bind(builder.Configuration.GetSection("GetAnIdentity"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services
                .AddTransient<ClientCredentialsBearerTokenDelegatingHandler>()
                .AddHttpClient<ClientCredentialsBearerTokenDelegatingHandler>();

            builder.Services
                .AddHttpClient<IGetAnIdentityApiClient, GetAnIdentityApiClient>((sp, httpClient) =>
                {
                    var options = sp.GetRequiredService<IOptions<GetAnIdentityOptions>>();
                    httpClient.BaseAddress = new Uri(options.Value.BaseAddress);
                })
                .AddHttpMessageHandler<ClientCredentialsBearerTokenDelegatingHandler>();
        }

        return builder;
    }
}
