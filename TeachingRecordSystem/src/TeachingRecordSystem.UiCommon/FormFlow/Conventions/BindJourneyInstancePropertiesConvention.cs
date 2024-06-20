using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TeachingRecordSystem.UiCommon.FormFlow.Conventions;

public class BindJourneyInstancePropertiesConvention : IPageApplicationModelConvention
{
    public void Apply(PageApplicationModel model)
    {
        var journeyInstanceProperties = model.HandlerProperties.Where(p =>
            p.ParameterType == typeof(JourneyInstance) ||
                p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition() == typeof(JourneyInstance<>));

        foreach (var journeyInstanceProperty in journeyInstanceProperties)
        {
            journeyInstanceProperty.BindingInfo ??= new BindingInfo();
        }
    }
}
