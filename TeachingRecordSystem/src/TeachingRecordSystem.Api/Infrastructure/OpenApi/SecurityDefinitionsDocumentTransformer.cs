using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public class SecurityDefinitionsDocumentTransformer(IConfiguration configuration) : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();

        document.Components.SecuritySchemes[SecuritySchemes.ApiKey] = new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Name = "Authorization",
            Scheme = "Bearer",
            Type = SecuritySchemeType.Http
        };

        document.Components.SecuritySchemes[SecuritySchemes.GetAnIdentityAccessToken] = new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Scheme = "Bearer",
            Type = SecuritySchemeType.OpenIdConnect,
            OpenIdConnectUrl = new Uri(configuration.GetRequiredValue("GetAnIdentity:BaseAddress") + ".well-known/openid-configuration")
        };

        return Task.CompletedTask;
    }
}
