using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using TeachingRecordSystem.Core.ApiSchema.V3;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

internal class AddWebhookMessagesTransformer(string minorVersion) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var messagesNamespace = $"TeachingRecordSystem.Core.ApiSchema.V3.V{minorVersion}.WebhookData";

        var messageTypes = typeof(IWebhookMessageData).Assembly.GetTypes()
            .Where(t => t.Namespace == messagesNamespace && t.IsAssignableTo(typeof(IWebhookMessageData)));

        document.Webhooks ??= new Dictionary<string, IOpenApiPathItem>();

        foreach (var messageType in messageTypes)
        {
            var cloudEventName = messageType.GetProperty(nameof(IWebhookMessageData.CloudEventType))?.GetValue(null) as string ??
                throw new InvalidOperationException($"Webhook message type {messageType.FullName} does not have a valid CloudEventType property.");

            var schema = await context.GetOrCreateSchemaAsync(messageType, cancellationToken: cancellationToken);

            var path = new OpenApiPathItem
            {
                Operations = new Dictionary<HttpMethod, OpenApiOperation>
                {
                    [HttpMethod.Post] = new()
                    {
                        RequestBody = new OpenApiRequestBody
                        {
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                ["application/json"] = new()
                                {
                                    Schema = schema
                                }
                            }
                        },
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse
                            {
                                Description = "Success"
                            }
                        }
                    }
                }
            };

            document.Webhooks.Add(cloudEventName, path);
        }
    }
}
