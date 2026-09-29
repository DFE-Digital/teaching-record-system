using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using TeachingRecordSystem.Core.ApiSchema.V3;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

internal class AddWebhookMessagesTransformer(string minorVersion) : IOpenApiDocumentTransformer
{
    // Set by Microsoft.AspNetCore.OpenApi on schemas for types that it would move into components
    private const string SchemaIdMetadataKey = "x-schema-id";

    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // Webhook messages are delivered using the schema from the most recent version at or before the endpoint's
        // version, so document every message an endpoint on this version can receive, not just the ones this
        // version introduced. The `ce-dataschema` on a delivered message points at this document.
        var eventMapperRegistry = context.ApplicationServices.GetRequiredService<EventMapperRegistry>();
        var messageTypes = eventMapperRegistry.GetDataTypesForApiVersion(minorVersion);

        document.Webhooks ??= new Dictionary<string, IOpenApiPathItem>();

        foreach (var messageType in messageTypes)
        {
            var cloudEventName = messageType.GetProperty(nameof(IWebhookMessageData.CloudEventType))?.GetValue(null) as string ??
                throw new InvalidOperationException($"Webhook message type {messageType.FullName} does not have a valid CloudEventType property.");

            var schema = await context.GetOrCreateSchemaAsync(messageType, cancellationToken: cancellationToken);
            document.AddComponent(messageType.Name, ReferenceNestedComponents(schema, document));

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
                                    Schema = new OpenApiSchemaReference(messageType.Name, document)
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

    // The framework only moves nested objects and enums into components for schemas reached from operations, so do the
    // same here, otherwise every webhook message would repeat the full definitions of the types it contains.
    private static IOpenApiSchema ReferenceNestedComponents(IOpenApiSchema schema, OpenApiDocument document)
    {
        if (schema is not OpenApiSchema inlineSchema)
        {
            return schema;
        }

        if (inlineSchema.Properties is { } properties)
        {
            foreach (var name in properties.Keys.ToArray())
            {
                properties[name] = ReferenceComponent(properties[name], document);
            }
        }

        if (inlineSchema.Items is { } items)
        {
            inlineSchema.Items = ReferenceComponent(items, document);
        }

        if (inlineSchema.AdditionalProperties is { } additionalProperties)
        {
            inlineSchema.AdditionalProperties = ReferenceComponent(additionalProperties, document);
        }

        foreach (var schemas in new[] { inlineSchema.AllOf, inlineSchema.AnyOf, inlineSchema.OneOf })
        {
            for (var i = 0; i < (schemas?.Count ?? 0); i++)
            {
                schemas![i] = ReferenceComponent(schemas[i], document);
            }
        }

        return inlineSchema;
    }

    private static IOpenApiSchema ReferenceComponent(IOpenApiSchema schema, OpenApiDocument document)
    {
        schema = ReferenceNestedComponents(schema, document);

        if (schema is not OpenApiSchema { Type: var type } inlineSchema ||
            inlineSchema.Metadata?.TryGetValue(SchemaIdMetadataKey, out var schemaIdObj) != true ||
            schemaIdObj is not string schemaId ||
            (type?.HasFlag(JsonSchemaType.Object) != true && inlineSchema.Enum is not { Count: > 0 }))
        {
            return schema;
        }

        // A nullable object is a nullable reference to a non-nullable component
        var isNullableObject = type is { } t && t.HasFlag(JsonSchemaType.Object) && t.HasFlag(JsonSchemaType.Null);
        if (isNullableObject)
        {
            inlineSchema = (OpenApiSchema)inlineSchema.CreateShallowCopy();
            inlineSchema.Type = type & ~JsonSchemaType.Null;
        }

        var componentId = GetOrAddComponent(schemaId, inlineSchema, document);
        var reference = new OpenApiSchemaReference(componentId, document);

        return isNullableObject ? new OpenApiSchema { OneOf = [new OpenApiSchema { Type = JsonSchemaType.Null }, reference] } : reference;
    }

    private static string GetOrAddComponent(string schemaId, OpenApiSchema schema, OpenApiDocument document)
    {
        var serialized = Serialize(schema);

        // Different types can share a schema ID (e.g. DTOs with the same name from different versions), so only reuse an
        // existing component if it's identical
        for (var suffix = 1; ; suffix++)
        {
            var componentId = suffix == 1 ? schemaId : $"{schemaId}{suffix}";

            if (document.Components?.Schemas?.TryGetValue(componentId, out var existing) != true)
            {
                document.AddComponent(componentId, schema);
                return componentId;
            }

            if (Serialize(existing!) == serialized)
            {
                return componentId;
            }
        }
    }

    private static string Serialize(IOpenApiSchema schema)
    {
        using var writer = new StringWriter();
        schema.SerializeAsV31(new OpenApiJsonWriter(writer));
        return writer.ToString();
    }
}
