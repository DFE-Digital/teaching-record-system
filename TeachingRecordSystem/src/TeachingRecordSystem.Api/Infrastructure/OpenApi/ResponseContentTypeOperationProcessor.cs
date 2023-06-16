using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class ResponseContentTypeOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        // Remove invalid Content-Types for a given response.
        // By convention, success responses are application/json and error responses are application/problem+json.

        foreach (var (key, response) in context.OperationDescription.Operation.Responses)
        {
            foreach (var contentType in response.Content.Keys.ToArray())
            {
                if (IsSuccessResponse() && contentType == "application/problem+json" ||
                    IsClientErrorResponse() && contentType == "application/json")
                {
                    response.Content.Remove(contentType);
                }

                bool IsSuccessResponse() => int.TryParse(key, out var statusCode) && statusCode >= 200 && statusCode < 300;

                bool IsClientErrorResponse() => key == "400";
            }
        }

        return true;
    }
}
