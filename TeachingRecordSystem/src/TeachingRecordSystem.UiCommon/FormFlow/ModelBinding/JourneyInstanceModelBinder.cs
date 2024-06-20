using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace TeachingRecordSystem.UiCommon.FormFlow.ModelBinding;

internal class JourneyInstanceModelBinder : IModelBinder
{
    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelType = bindingContext.ModelType;

        var requestServices = bindingContext.HttpContext.RequestServices;
        var journeyInstanceProvider = requestServices.GetRequiredService<JourneyInstanceProvider>();

        var instance = await journeyInstanceProvider.GetInstanceAsync(bindingContext.ActionContext);

        if (instance is not null)
        {
            if (modelType.IsGenericType)
            {
                var stateType = modelType.GetGenericArguments()[0];
                if (stateType != instance.StateType)
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                    return;
                }
            }

            bindingContext.ValidationState.Add(instance, new ValidationStateEntry() { SuppressValidation = true });
            bindingContext.Result = ModelBindingResult.Success(instance);
        }
    }

    internal static bool IsJourneyInstanceType(Type type) =>
        type == typeof(JourneyInstance) ||
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(JourneyInstance<>);
}

internal class JourneyInstanceModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        var modelType = context.Metadata.ModelType;

        if (modelType == typeof(JourneyInstance) ||
            modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(JourneyInstance<>))
        {
            return new JourneyInstanceModelBinder();
        }

        return null;
    }
}
