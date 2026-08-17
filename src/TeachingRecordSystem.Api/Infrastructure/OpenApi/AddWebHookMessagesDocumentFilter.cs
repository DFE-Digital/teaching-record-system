using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TeachingRecordSystem.Core.Services.Webhooks;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class AddWebHookMessagesDocumentFilter(EventMapperRegistry eventMapperRegistry) : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        (int majorVersion, string? minorVersion) = OpenApiDocumentHelper.GetVersionsFromVersionName(swaggerDoc.Info.Version);

        if (majorVersion != 3 || minorVersion is null)
        {
            return;
        }

        // Webhook messages are delivered using the schema from the most recent version at or before the endpoint's
        // version, so document every message an endpoint on this version can receive, not just the ones this
        // version introduced. The `ce-dataschema` on a delivered message points at this document.
        foreach (var messageType in eventMapperRegistry.GetDataTypesForApiVersion(minorVersion))
        {
            context.SchemaGenerator.GenerateSchema(messageType, context.SchemaRepository);
        }
    }
}
