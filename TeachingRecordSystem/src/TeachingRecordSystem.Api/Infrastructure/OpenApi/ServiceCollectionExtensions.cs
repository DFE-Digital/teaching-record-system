using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using TeachingRecordSystem.Api.Infrastructure.ApplicationModel;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using Constants = TeachingRecordSystem.Api.Infrastructure.ApplicationModel.Constants;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IApiDescriptionProvider, HybridBodyApiDescriptionProvider>();

        foreach (var (version, minorVersion) in VersionRegistry.GetAllVersions(configuration))
        {
            var documentName = OpenApiDocumentHelper.GetDocumentName(version, minorVersion);
            var capturedVersion = version;
            var capturedMinorVersion = minorVersion;

            services.AddOpenApi(documentName, options =>
            {
                options.ShouldInclude = (apiDescription) =>
                {
                    var properties = apiDescription.ActionDescriptor.Properties;

                    if (properties.TryGetValue(Constants.VersionPropertyKey, out var versionObj) &&
                        versionObj is int opVersion && opVersion == capturedVersion)
                    {
                        if (properties.TryGetValue(Constants.MinorVersionsPropertyKey, out var minorVersionsObj) &&
                            minorVersionsObj is ApiMinorVersionsMetadata minorVersionsMetadata)
                        {
                            return minorVersionsMetadata.MinorVersions.Contains(capturedMinorVersion);
                        }
                        else
                        {
                            return capturedMinorVersion == null;
                        }
                    }

                    return false;
                };

                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Version = OpenApiDocumentHelper.GetVersionName(capturedVersion, capturedMinorVersion);
                    document.Info.Title = OpenApiDocumentHelper.Title;
                    return Task.CompletedTask;
                });

                options.AddDocumentTransformer<SecurityDefinitionsDocumentTransformer>();
                options.AddDocumentTransformer<MinorVersionHeaderDocumentTransformer>();
                options.AddDocumentTransformer<AddWebHookMessagesDocumentTransformer>();
                options.AddOperationTransformer<ContentTypesOperationTransformer>();
                options.AddOperationTransformer<AddSecuritySchemeOperationTransformer>();
                options.AddSchemaTransformer<RemoveExcludedEnumOptionsSchemaTransformer>();
                options.AddSchemaTransformer<RemoveEnumValuesForFlagsEnumSchemaTransformer>();
                options.AddSchemaTransformer<UnwrapOptionSchemaTransformer>();
                options.AddSchemaTransformer<OneOfSchemaTransformer>();
            });
        }

        services.AddSingleton<IStartupFilter, OpenApiEndpointsStartupFilter>();

        return services;
    }
}

public class OpenApiEndpointsStartupFilter(IConfiguration configuration) : IStartupFilter
{
    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        next(app);

        foreach (var (version, minorVersion) in VersionRegistry.GetAllVersions(configuration))
        {
            var documentName = OpenApiDocumentHelper.GetDocumentName(version, minorVersion);
            app.MapOpenApi($"/{OpenApiDocumentHelper.DocumentRouteTemplate.Replace("{documentName}", documentName)}")
                .WithName($"OpenApi_{documentName}");
        }

        app.MapScalarApiReference(options =>
        {
            options.WithPreferredScheme("ApiKey");
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);

            foreach (var (version, minorVersion) in VersionRegistry.GetAllVersions(configuration).Reverse())
            {
                var documentName = OpenApiDocumentHelper.GetDocumentName(version, minorVersion);
                var documentUrl = "/" + OpenApiDocumentHelper.DocumentRouteTemplate.Replace("{documentName}", documentName);
                options.WithEndpointPrefix(documentUrl);
            }
        });
    };
}
