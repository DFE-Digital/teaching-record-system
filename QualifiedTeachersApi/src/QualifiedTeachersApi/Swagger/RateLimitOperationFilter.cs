using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace QualifiedTeachersApi.Swagger;

public class RateLimitOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var problemDetailsSchema = context.SchemaGenerator.GenerateSchema(modelType: typeof(ProblemDetails), context.SchemaRepository);

        operation.Responses.Add("429", new OpenApiResponse()
        {
            Content =
            {
                { "application/problem+json", new OpenApiMediaType() { Schema = problemDetailsSchema } }
            },
            Description = ReasonPhrases.GetReasonPhrase(429)
        });
    }
}
