using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace QualifiedTeachersApi.Infrastructure.OpenApi;

public class AddTooManyRequestsResponseOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var document429Response = context.Document.Responses.SingleOrDefault(r => r.Key == "429").Value;

        if (document429Response is null)
        {
            if (!context.SchemaResolver.HasSchema(typeof(ProblemDetails), isIntegerEnumeration: false))
            {
                context.SchemaResolver.AddSchema(
                    typeof(ProblemDetails),
                    isIntegerEnumeration: false,
                    context.SchemaGenerator.Generate(typeof(ProblemDetails)));
            }

            var problemDetailsSchema = context.SchemaResolver.GetSchema(typeof(ProblemDetails), isIntegerEnumeration: false);

            document429Response = new OpenApiResponse()
            {
                Content =
                {
                    {
                        "application/problem+json",
                        new OpenApiMediaType()
                        {
                            Schema = new NJsonSchema.JsonSchema() { Reference = problemDetailsSchema }
                        }
                    }
                },
                Description = ReasonPhrases.GetReasonPhrase(429)
            };

            context.Document.Responses.Add("429", document429Response);
        }

        context.OperationDescription.Operation.Responses.Add("429", new OpenApiResponse()
        {
            Reference = document429Response
        });

        return true;
    }
}
