using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSwaggerGen(options =>
        {
            options.EnableAnnotations();

            options.DocInclusionPredicate((docName, apiDescription) => apiDescription.GroupName == docName);

            options.AddSecurityDefinition(SecuritySchemes.ApiKey, new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Name = "Authorization",
                Scheme = "Bearer",
                Type = SecuritySchemeType.Http
            });

            options.AddSecurityDefinition(SecuritySchemes.GetAnIdentityAccessToken, new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Scheme = "Bearer",
                Type = SecuritySchemeType.OpenIdConnect,
                OpenIdConnectUrl = new Uri(configuration.GetRequiredValue("GetAnIdentity:BaseAddress") + ".well-known/openid-configuration")
            });

            options.SupportNonNullableReferenceTypes();
            options.SchemaFilter<RemoveExcludedEnumOptionsSchemaFilter>();
            options.SchemaFilter<RemoveEnumValuesForFlagsEnumSchemaFilter>();
            options.OperationFilter<ContentTypesOperationFilter>();
            options.OperationFilter<AddSecuritySchemeOperationFilter>();
            options.DocumentFilter<MinorVersionHeaderDocumentFilter>();

            foreach (var (version, minorVersion) in VersionRegistry.GetAllVersions())
            {
                options.SwaggerDoc(
                    OpenApiDocumentHelper.GetDocumentName(version, minorVersion),
                    new OpenApiInfo()
                    {
                        Version = OpenApiDocumentHelper.GetVersionName(version, minorVersion),
                        Title = OpenApiDocumentHelper.Title
                    });
            }
        });

        services.Decorate<ISerializerDataContractResolver, UnwrapOptionSerializerDataContractResolver>();

        services.AddSingleton<IStartupFilter, OpenApiEndpointsStartupFilter>();

        return services;
    }
}

public class OpenApiEndpointsStartupFilter : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        next(app);

        app.UseSwagger(o => o.RouteTemplate = OpenApiDocumentHelper.DocumentRouteTemplate);

        app.UseSwaggerUI(options =>
        {
            foreach (var (version, minorVersion) in VersionRegistry.GetAllVersions())
            {
                var documentName = OpenApiDocumentHelper.GetDocumentName(version, minorVersion);
                options.SwaggerEndpoint(OpenApiDocumentHelper.DocumentRouteTemplate.Replace("{documentName}", documentName), documentName);
            }

            options.EnablePersistAuthorization();
        });
    };
}
