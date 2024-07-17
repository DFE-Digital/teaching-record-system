using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Options;
using NSign;
using NSign.Client;
using TeachingRecordSystem.Api.Infrastructure.Json;
using TeachingRecordSystem.Api.Infrastructure.OpenApi;

namespace TeachingRecordSystem.Api.Webhooks;

public class WebhookMessageSender(HttpClient httpClient, IOptions<WebhookOptions> optionsAccessor)
{
    private const string DataContentType = "application/json";

    private readonly CloudEventFormatter _formatter = new JsonEventFormatter(
        new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            {
                Modifiers = { Modifiers.OptionProperties }
            }
        },
        new JsonDocumentOptions());

    public async Task SendMessage(WebhookMessage message, Uri endpoint, CancellationToken cancellationToken)
    {
        var openApiDocumentName = OpenApiDocumentHelper.GetDocumentName(3, message.MinorVersion);
        var swaggerEndpoint = OpenApiDocumentHelper.DocumentRouteTemplate.Replace("{documentName}", openApiDocumentName);
        var dataSchema = new Uri(optionsAccessor.Value.CanonicalDomain + swaggerEndpoint, UriKind.Absolute);

        var source = new Uri(optionsAccessor.Value.CanonicalDomain);

        var cloudEvent = new CloudEvent()
        {
            Id = message.CloudEventId,
            Source = source,
            Type = message.CloudEventType,
            DataContentType = DataContentType,
            DataSchema = dataSchema,
            Time = message.Timestamp,
            Data = message.Data,
        };

        var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = cloudEvent.ToHttpContent(ContentMode.Structured, _formatter)
        };

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}

public static class Extensions  // TODO move
{
    public static IServiceCollection ConfigureWebHookSender(this IServiceCollection services)
    {
        services.AddSingleton<WebhookMessageSender>();

        // TODO DOn't configure AddContentDigestOptions or MessageSigningOptions globally

        services.Configure<AddContentDigestOptions>(options => options.WithHash(AddContentDigestOptions.Hash.Sha256));

        services.Configure<MessageSigningOptions>(options =>
        {
            //...
        });

        services
            .AddHttpClient<WebhookMessageSender>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Teaching Record System");
            })
            .AddContentDigestAndSigningHandlers();

        //services.AddSingleton<ISigner>()

        return services;
    }
}
