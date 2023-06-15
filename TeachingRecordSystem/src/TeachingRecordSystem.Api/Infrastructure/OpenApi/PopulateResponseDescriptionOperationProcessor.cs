using Microsoft.AspNetCore.WebUtilities;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class PopulateResponseDescriptionOperationProcessor : IDocumentProcessor, IOperationProcessor
{
    public void Process(DocumentProcessorContext context)
    {
        foreach (var (key, response) in context.Document.Responses)
        {
            if (string.IsNullOrEmpty(response.Description) && int.TryParse(key, out var responseCode))
            {
                response.Description = ReasonPhrases.GetReasonPhrase(responseCode);
            }
        }
    }

    public bool Process(OperationProcessorContext context)
    {
        foreach (var (key, response) in context.OperationDescription.Operation.Responses)
        {
            if (string.IsNullOrEmpty(response.Description) && int.TryParse(key, out var responseCode))
            {
                response.Description = ReasonPhrases.GetReasonPhrase(responseCode);
            }
        }

        return true;
    }
}
