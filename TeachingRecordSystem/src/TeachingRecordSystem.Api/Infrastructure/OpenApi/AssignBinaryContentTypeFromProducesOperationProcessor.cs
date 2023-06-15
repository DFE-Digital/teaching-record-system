using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class AssignBinaryContentTypeFromProducesOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        // NSwag assigns all binary responses to application/octet-stream.
        // If we've been more specific (by using, say, [ProducesResponseType(...)] then use that type instead.

        if (context is AspNetCoreOperationProcessorContext aspNetCoreContext)
        {
            foreach (var (responseKey, response) in context.OperationDescription.Operation.Responses)
            {
                if (response.Content.Count == 1 && response.Content.ContainsKey("application/octet-stream"))
                {
                    var apiResponseType = aspNetCoreContext.ApiDescription.SupportedResponseTypes.Single(r => r.StatusCode.ToString() == responseKey);
                    response.Content.Clear();

                    foreach (var responseFormat in apiResponseType.ApiResponseFormats)
                    {
                        response.Content.Add(responseFormat.MediaType, new NSwag.OpenApiMediaType()
                        {
                            Schema = new NJsonSchema.JsonSchema()
                            {
                                Type = NJsonSchema.JsonObjectType.String,
                                Format = "binary"
                            }
                        });
                    }
                }
            }
        }

        return true;
    }
}
