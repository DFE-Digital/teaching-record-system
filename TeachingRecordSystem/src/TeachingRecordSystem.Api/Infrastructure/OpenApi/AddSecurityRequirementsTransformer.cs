using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

internal class AddSecurityRequirementsTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var actionDescriptor = context.Description.ActionDescriptor;

        var authorizeAttributes = actionDescriptor.EndpointMetadata.OfType<AuthorizeAttribute>().ToArray();
        if (authorizeAttributes.Length != 1)
        {
            throw new NotSupportedException("Cannot derive security scheme for operation.");
        }

        var authorizationPolicy = authorizeAttributes.Single().Policy;
        OpenApiSecurityRequirement? requirement;

        if (authorizationPolicy is AuthorizationPolicies.ApiKey)
        {
            requirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference(SecuritySchemes.ApiKey),
                    []
                }
            };
        }
        else if (authorizationPolicy == AuthorizationPolicies.IdentityUserWithTrn)
        {
            requirement = new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference(SecuritySchemes.GetAnIdentityAccessToken),
                    ["trn"]  // The OAuth scopes required
                }
            };
        }
        else
        {
            throw new NotSupportedException("Cannot derive security scheme for operation.");
        }

        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(requirement);

        return Task.CompletedTask;
    }
}
