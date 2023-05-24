using Microsoft.AspNetCore.Authorization;
using NSwag;
using NSwag.Generation.AspNetCore;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;
using QualifiedTeachersApi.Infrastructure.Security;

namespace QualifiedTeachersApi.Infrastructure.OpenApi;

public class AddSecuritySchemeOperationProcessor : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        if (context is AspNetCoreOperationProcessorContext aspNetCoreContext)
        {
            var actionDescriptor = aspNetCoreContext.ApiDescription.ActionDescriptor;
            var authorizeAttributes = actionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().ToArray();

            if (authorizeAttributes.Length != 1)
            {
                throw new NotSupportedException("Cannot derive security scheme for operation.");
            }

            var authorizationPolicy = authorizeAttributes.Single().Policy;
            OpenApiSecurityRequirement? requirement;

            if (authorizationPolicy == AuthorizationPolicies.ApiKey)
            {
                requirement = new OpenApiSecurityRequirement()
                {
                    {
                        SecuritySchemes.ApiKey,
                        Array.Empty<string>()
                    }
                };
            }
            else if (authorizationPolicy == AuthorizationPolicies.IdentityUserWithTrn)
            {
                requirement = new OpenApiSecurityRequirement()
                {
                    {
                        SecuritySchemes.GetAnIdentityAccessToken,
                        Array.Empty<string>()
                    }
                };
            }
            else
            {
                throw new NotSupportedException("Cannot derive security scheme for operation.");
            }

            context.OperationDescription.Operation.Security ??= new List<OpenApiSecurityRequirement>();
            context.OperationDescription.Operation.Security.Add(requirement);
        }

        return true;
    }
}
