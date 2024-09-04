using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class AddSecuritySchemeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var actionDescriptor = context.ApiDescription.ActionDescriptor;

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
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = SecuritySchemes.ApiKey },
                    },
                    []
                }
            };
        }
        else if (authorizationPolicy == AuthorizationPolicies.IdentityUserWithTrn)
        {
            requirement = new OpenApiSecurityRequirement()
            {
                {
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = SecuritySchemes.GetAnIdentityAccessToken },
                    },
                    []
                }
            };
        }
        else
        {
            throw new NotSupportedException("Cannot derive security scheme for operation.");
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(requirement);
    }
}
