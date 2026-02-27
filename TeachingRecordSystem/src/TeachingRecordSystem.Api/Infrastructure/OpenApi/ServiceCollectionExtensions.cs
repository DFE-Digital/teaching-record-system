using Microsoft.AspNetCore.Mvc.ApiExplorer;
using TeachingRecordSystem.Api.Infrastructure.ApplicationModel;
using TeachingRecordSystem.Api.Infrastructure.ModelBinding;
using Constants = TeachingRecordSystem.Api.Infrastructure.ApplicationModel.Constants;

namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IApiDescriptionProvider, HybridBodyApiDescriptionProvider>();

        foreach (var (majorVersion, minorVersion) in VersionRegistry.GetAllVersions(configuration))
        {
            var documentName = OpenApiDocumentHelper.GetDocumentName(majorVersion, minorVersion);

            services.AddOpenApi(documentName, options =>
            {
                options.AddDocumentTransformer((doc, _, _) =>
                {
                    doc.Info.Title = OpenApiDocumentHelper.Title;
                    doc.Info.Version = documentName;

                    doc.Servers = null;
                    doc.Tags?.Clear();

                    return Task.CompletedTask;
                });

                options.AddDocumentTransformer(new AddSecuritySchemesTransformer(configuration));
                options.AddOperationTransformer(new AddSecurityRequirementsTransformer());
                options.AddOperationTransformer(new RemoveOperationTagsTransformer());
                options.AddOperationTransformer(new SetContentTypesTransformer());

                if (minorVersion is not null)
                {
                    options.AddDocumentTransformer(new AddWebhookMessagesTransformer(minorVersion));
                    options.AddDocumentTransformer(new AddMinorVersionHeaderTransformer(minorVersion));
                }

                // Only include endpoints in the OpenAPI document that match the major and minor version of the document
                options.ShouldInclude = apiDescription =>
                {
                    var properties = apiDescription.ActionDescriptor.Properties;

                    if (properties.TryGetValue(Constants.VersionPropertyKey, out var versionObj) && versionObj is int apiMajorVersion)
                    {
                        if (apiMajorVersion != majorVersion)
                        {
                            return false;
                        }

                        if (properties.TryGetValue(Constants.MinorVersionsPropertyKey, out var minorVersionsObj) &&
                            minorVersionsObj is ApiMinorVersionsMetadata minorVersionsMetadata)
                        {
                            foreach (var apiMinorVersion in minorVersionsMetadata.MinorVersions)
                            {
                                if (apiMinorVersion == minorVersion)
                                {
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }

                    return false;
                };
            });
        }

        return services;
    }
}
