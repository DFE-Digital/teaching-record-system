using NSwag.Examples;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExampleProviders(typeof(Program).Assembly);

        foreach (var version in Api.Constants.Versions)
        {
            services.AddOpenApiDocument((settings, provider) =>
            {
                settings.DocumentName = OpenApiDocumentHelper.GetDocumentName(version);
                settings.Version = OpenApiDocumentHelper.GetVersionName(version);
                settings.Title = "Teaching Record System API";
                settings.ApiGroupNames = new[] { OpenApiDocumentHelper.GetVersionName(version) };
                //settings.AddExamples(provider);   // Broken with the current NSwag.Examples library
                settings.SchemaSettings.TypeNameGenerator = new GeneratedCrmTypeNameGenerator(settings.SchemaSettings.TypeNameGenerator);

                settings.DocumentProcessors.Add(new PopulateResponseDescriptionOperationProcessor());

                settings.SchemaSettings.SchemaProcessors.Add(new RemoveCompositeValuesFromFlagsEnumSchemaProcessor());
                settings.SchemaSettings.SchemaProcessors.Add(new RemoveExcludedEnumOptionsSchemaProcessor());

                settings.OperationProcessors.Add(new ResponseContentTypeOperationProcessor());
                settings.OperationProcessors.Add(new PopulateResponseDescriptionOperationProcessor());
                settings.OperationProcessors.Add(new AssignBinaryContentTypeFromProducesOperationProcessor());

                settings.SchemaSettings.ReflectionService = new UnwrapOptionTypesReflectionService();

                settings.AddSecurity(SecuritySchemes.ApiKey, new NSwag.OpenApiSecurityScheme()
                {
                    In = NSwag.OpenApiSecurityApiKeyLocation.Header,
                    Name = "Authorization",
                    Scheme = "Bearer",
                    Type = NSwag.OpenApiSecuritySchemeType.Http
                });

                if (version == 3)
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
