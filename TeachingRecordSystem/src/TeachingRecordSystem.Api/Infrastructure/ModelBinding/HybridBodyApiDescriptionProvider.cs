#nullable disable
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TeachingRecordSystem.Api.Infrastructure.ModelBinding;

public class HybridBodyApiDescriptionProvider : IApiDescriptionProvider
{
    private readonly IModelMetadataProvider _modelMetadataProvider;

    public HybridBodyApiDescriptionProvider(IModelMetadataProvider modelMetadataProvider)
    {
        _modelMetadataProvider = modelMetadataProvider;
    }

    public int Order => 0;

    public void OnProvidersExecuted(ApiDescriptionProviderContext context)
    {
        foreach (var apiDescription in context.Results)
        {
            foreach (var parameterDescription in apiDescription.ParameterDescriptions.ToArray())
            {
                if (parameterDescription.BindingInfo?.BindingSource == BindingSource.Body)
                {
                    var bodyParameterType = parameterDescription.Type;

                    foreach (var property in bodyParameterType.GetProperties())
                    {
                        var fromQueryAttribute = property.GetCustomAttribute<FromQueryAttribute>();
                        var fromRouteAttribute = property.GetCustomAttribute<FromRouteAttribute>();

                        if (fromQueryAttribute != null || fromRouteAttribute != null)
                        {
                            var modelMetadata = _modelMetadataProvider.GetMetadataForProperty(bodyParameterType, property.Name);
                            var bindingInfo = BindingInfo.GetBindingInfo(property.GetCustomAttributes(), modelMetadata);

                            var name = ModelNames.CreatePropertyModelName(
                                prefix: string.Empty,
                                propertyName: !string.IsNullOrEmpty(modelMetadata.BinderModelName) ? modelMetadata.BinderModelName : modelMetadata.PropertyName);

                            if (fromRouteAttribute != null)
                            {
                                // We should have an existing parameter for this property (that comes from the route template).
                                // If so, enrich it rather than replacing it

                                var existingRouteParameter = apiDescription.ParameterDescriptions
                                    .SingleOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && p.Source == BindingSource.Path);

                                if (existingRouteParameter != null)
                                {
                                    existingRouteParameter.ModelMetadata = modelMetadata;
                                    existingRouteParameter.Type = modelMetadata.ModelType;

                                    continue;
                                }
                            }

                            apiDescription.ParameterDescriptions.Add(new ApiParameterDescription()
                            {
                                BindingInfo = bindingInfo,
                                Name = name,
                                Source = modelMetadata.BindingSource,
                                Type = modelMetadata.ModelType,
                                ModelMetadata = modelMetadata
                            });
                        }
                    }
                }
            }
        }
    }

    public void OnProvidersExecuting(ApiDescriptionProviderContext context)
    {
    }
}
