using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class AddWebHookMessagesDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        (int majorVersion, string? minorVersion) = OpenApiDocumentHelper.GetVersionsFromVersionName(swaggerDoc.Info.Version);

        if (majorVersion == 3)
        {
            var messagesNamespace = $"TeachingRecordSystem.Api.V3.V{minorVersion}.WebHookMessages";

            var messageTypes = typeof(AddWebHookMessagesDocumentFilter).Assembly.GetTypes()
                .Where(t => t.Namespace == messagesNamespace);

            foreach (var messageType in messageTypes)
            {
                context.SchemaGenerator.GenerateSchema(messageType, context.SchemaRepository);
            }
        }
    }
}
