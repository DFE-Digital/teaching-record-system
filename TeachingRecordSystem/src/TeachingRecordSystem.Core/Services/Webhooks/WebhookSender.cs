using System.Diagnostics;
using CloudNative.CloudEvents;
using CloudNative.CloudEvents.Http;
using CloudNative.CloudEvents.SystemTextJson;
using Microsoft.Extensions.Options;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public class WebhookSender(HttpClient httpClient, IOptions<WebhookOptions> optionsAccessor)
{
    private const string DataContentType = "application/json; charset=utf-8";

    private readonly CloudEventFormatter _formatter = new JsonEventFormatter();

    public async Task SendMessageAsync(WebhookMessage message, CancellationToken cancellationToken = default)
    {
        Debug.Assert(message.WebhookEndpoint is not null);

        var openApiDocumentName = OpenApiDocumentHelper.GetDocumentName(3, message.ApiVersion);
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

        var request = new HttpRequestMessage(HttpMethod.Post, message.WebhookEndpoint.Address)
        {
            Content = cloudEvent.ToHttpContent(ContentMode.Binary, _formatter)
        };

        var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
