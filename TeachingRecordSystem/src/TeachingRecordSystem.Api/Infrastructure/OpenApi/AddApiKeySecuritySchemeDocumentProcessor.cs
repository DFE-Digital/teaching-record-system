using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class AddApiKeySecuritySchemeDocumentProcessor : IDocumentProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        context.Document.Security.Add(new NSwag.OpenApiSecurityRequirement()
        {
            {
                SecuritySchemes.ApiKey,
                Array.Empty<string>()
            }
        });
    }
}
