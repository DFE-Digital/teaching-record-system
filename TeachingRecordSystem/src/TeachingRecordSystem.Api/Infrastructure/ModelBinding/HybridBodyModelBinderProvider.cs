#nullable disable
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace TeachingRecordSystem.Api.Infrastructure.ModelBinding;

public class HybridBodyModelBinderProvider : IModelBinderProvider
{
    private readonly BodyModelBinderProvider _innerProvider;

    public HybridBodyModelBinderProvider(BodyModelBinderProvider innerProvider)
    {
        _innerProvider = innerProvider;
    }

    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        var modelBinder = _innerProvider.GetBinder(context);

        if (modelBinder == null || context.BindingInfo.BindingSource != BindingSource.Body)
        {
            return modelBinder;
        }

        var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
        foreach (var property in context.Metadata.Properties)
        {
            var createPropertyBinder = property.BindingSource == BindingSource.Path ||
                                       property.BindingSource == BindingSource.Query;

            if (createPropertyBinder)
            {
                propertyBinders.Add(property, context.CreateBinder(property));
            }
        }

        return new HybridBodyModelBinder(modelBinder, propertyBinders);
    }

    private class HybridBodyModelBinder : IModelBinder
    {
        private readonly IModelBinder _innerModelBinder;
        private readonly Dictionary<ModelMetadata, IModelBinder> _propertyBinders;

        public HybridBodyModelBinder(IModelBinder innerModelBinder, Dictionary<ModelMetadata, IModelBinder> propertyBinders)
        {
            _innerModelBinder = innerModelBinder;
            _propertyBinders = propertyBinders;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            await _innerModelBinder.BindModelAsync(bindingContext);

            if (bindingContext.Result.IsModelSet)
            {
                foreach (var (property, propertyBinder) in _propertyBinders)
                {
                    var fieldName = property.PropertyName;
                    var modelName = ModelNames.CreatePropertyModelName(bindingContext.ModelName, fieldName);

                    ModelBindingResult result;
                    using (bindingContext.EnterNestedScope(property, fieldName, modelName, model: null))
                    {
                        await propertyBinder.BindModelAsync(bindingContext);
                        result = bindingContext.Result;
                    }

                    property.PropertySetter(
                        bindingContext.Result.Model,
                        result.IsModelSet ? result.Model : CreateDefaultValue(property.ModelType));
                }
            }

            static object CreateDefaultValue(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
