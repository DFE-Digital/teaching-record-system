﻿using FastEndpoints.Swagger;
using NSwag.Examples;
using NSwag.Generation.AspNetCore;

namespace QualifiedTeachersApi.Infrastructure.OpenApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddExampleProviders(typeof(Program).Assembly);

        foreach (var version in Api.Constants.Versions)
        {
            services.SwaggerDocument(o =>
            {
                o.EnableJWTBearerAuth = false;
                o.EndpointFilter = ep => ep.Version.Current == version;
                o.ExcludeNonFastEndpoints = false;
                o.AutoTagPathSegmentIndex = 0;
                o.ShortSchemaNames = true;
                o.MinEndpointVersion = version;
                o.MaxEndpointVersion = version;

                o.DocumentSettings = settings =>
                {
                    settings.DocumentName = OpenApiDocumentHelper.GetDocumentName(version);
                    settings.Version = OpenApiDocumentHelper.GetVersionName(version);
                    settings.Title = "Qualified Teachers API";
                    settings.AddOperationFilter(o =>
                    {
                        // Include Controllers for this version and all Endpoints (EndpointFilter above applies filter for Endpoints)
                        return o.GetEndpointDefinition() is not null ||
                            (o is AspNetCoreOperationProcessorContext aspNetCoreOperationContext &&
                                aspNetCoreOperationContext.ApiDescription.GroupName == OpenApiDocumentHelper.GetVersionName(version));
                    });
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
                };
            });
        }

        services.AddSingleton<IStartupFilter, OpenApiEndpointsStartupFilter>();

        return services;
    }
}
