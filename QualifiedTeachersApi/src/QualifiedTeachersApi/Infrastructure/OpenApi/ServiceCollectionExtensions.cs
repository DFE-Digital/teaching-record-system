using Microsoft.Extensions.Options;
using NSwag.Examples;
using QualifiedTeachersApi.Services.GetAnIdentityApi;

namespace QualifiedTeachersApi.Infrastructure.OpenApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExampleProviders(typeof(Program).Assembly);

        foreach (var version in Api.Constants.Versions)
        {
            services.AddOpenApiDocument((settings, provider) =>
            {
                settings.DocumentName = $"Qualified Teachers API {version}";
                settings.Version = version;
                settings.Title = "Qualified Teachers API";
                settings.ApiGroupNames = new[] { version };
                settings.AddExamples(provider);
                settings.TypeNameGenerator = new GeneratedCrmTypeNameGenerator(settings.TypeNameGenerator);

                settings.DocumentProcessors.Add(new PopulateResponseDescriptionOperationProcessor());

                settings.SchemaProcessors.Add(new RemoveEnumValuesSchemaProcessor());

                settings.OperationProcessors.Add(new ResponseContentTypeOperationProcessor());
                settings.OperationProcessors.Add(new AddTooManyRequestsResponseOperationProcessor());
                settings.OperationProcessors.Add(new PopulateResponseDescriptionOperationProcessor());
                settings.OperationProcessors.Add(new AssignBinaryContentTypeFromProducesOperationProcessor());

                settings.SchemaGenerator = new HandleOptionJsonSchemaGenerator(settings);

                settings.AddSecurity(SecuritySchemes.ApiKey, new NSwag.OpenApiSecurityScheme()
                {
                    In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                    Name = "Authorization",
                    Scheme = "Bearer",
                    Type = NSwag.OpenApiSecuritySchemeType.Http
                });

                if (version == "v3")
                {
                    // Only V3 uses ID access tokens for auth
                    settings.AddSecurity(SecuritySchemes.GetAnIdentityAccessToken, new NSwag.OpenApiSecurityScheme()
                    {
                        In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                        Scheme = "Bearer",
                        Type = NSwag.OpenApiSecuritySchemeType.OpenIdConnect,
                        OpenIdConnectUrl = configuration["GetAnIdentity:BaseAddress"] + ".well-known/openid-configuration"
                    });

                    // Add operation-level security scheme instead of document-level
                    settings.OperationProcessors.Add(new AddSecuritySchemeOperationProcessor());
                }
                else
                {
                    settings.DocumentProcessors.Add(new AddApiKeySecuritySchemeDocumentProcessor());
                }
            });
        }

        services.AddSingleton<IStartupFilter, OpenApiEndpointsStartupFilter>();

        return services;
    }
}

public class OpenApiEndpointsStartupFilter : IStartupFilter
{
    private readonly IWebHostEnvironment _environment;
    private readonly IOptions<GetAnIdentityOptions> _identityOptionsAccessor;

    public OpenApiEndpointsStartupFilter(IWebHostEnvironment environment, IOptions<GetAnIdentityOptions> identityOptionsAccessor)
    {
        _environment = environment;
        _identityOptionsAccessor = identityOptionsAccessor;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        next(app);

        foreach (var version in Api.Constants.Versions)
        {
            app.UseOpenApi(settings =>
            {
                settings.DocumentName = $"Qualified Teachers API {version}";
                settings.Path = $"/swagger/{version}/swagger.json";

                settings.PostProcess = (document, request) =>
                {
                    document.Host = null;
                    document.Generator = null;
                    document.Servers.Clear();
                };
            });
        }

        if (_environment.IsDevelopment())
        {
            app.UseSwaggerUi3(settings =>
            {
                foreach (var version in Api.Constants.Versions)
                {
                    settings.SwaggerRoutes.Add(new NSwag.AspNetCore.SwaggerUi3Route(version, $"/swagger/{version}/swagger.json"));
                }

                settings.PersistAuthorization = true;

                settings.OAuth2Client = new NSwag.AspNetCore.OAuth2ClientSettings()
                {
                    ClientId = _identityOptionsAccessor.Value.ClientId,
                    ClientSecret = _identityOptionsAccessor.Value.ClientSecret,
                    UsePkceWithAuthorizationCodeGrant = true
                };
            });
        }
    };
}
