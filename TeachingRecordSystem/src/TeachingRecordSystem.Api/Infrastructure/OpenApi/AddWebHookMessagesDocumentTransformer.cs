using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class AddWebHookMessagesDocumentTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument swaggerDoc, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        (int majorVersion, string? minorVersion) = OpenApiDocumentHelper.GetVersionsFromVersionName(swaggerDoc.Info.Version);

        if (majorVersion == 3)
        {
            var messagesNamespace = $"TeachingRecordSystem.Api.V3.V{minorVersion}.WebHookMessages";

            var messageTypes = typeof(AddWebHookMessagesDocumentTransformer).Assembly.GetTypes()
                .Where(t => t.Namespace == messagesNamespace);

            foreach (var messageType in messageTypes)
            {
                // Generate a schema for each webhook message type to ensure they appear in the document
                var schemaName = messageType.Name;
                if (!swaggerDoc.Components.Schemas.ContainsKey(schemaName))
                {
                    try
                    {
                        // The schema service will handle generating the schema
                        var jsonTypeInfo = context.ApplicationServices.GetRequiredService<JsonSerializerOptions>()
                            .GetTypeInfo(messageType);
                        
                        if (jsonTypeInfo != null)
                        {
                            var schemaContext = new OpenApiSchemaTransformerContext(
                                jsonTypeInfo,
                                context.ApplicationServices,
                                context.DocumentName);
                            
                            // Request the schema to be generated
                            _ = context.SchemaService.GetOrCreateSchema(messageType, schemaContext);
                        }
                    }
                    catch
                    {
                        // If we can't generate the schema, skip it
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}
