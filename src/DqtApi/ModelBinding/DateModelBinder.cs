using System;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DqtApi.ModelBinding
{
    public class DateModelBinder : IModelBinder
    {       
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            var modelState = bindingContext.ModelState;
            modelState.SetModelValue(modelName, valueProviderResult);

            var metadata = bindingContext.ModelMetadata;
            var type = metadata.UnderlyingOrModelType;

            try
            {
                var value = valueProviderResult.FirstValue;

                object model;
                if (string.IsNullOrWhiteSpace(value))
                {
                    model = null;
                }
                else if (type == typeof(DateTime))
                {
                    model = DateTime.ParseExact(value, Constants.DateFormat, provider: null);
                }
                else if (type == typeof(DateOnly))
                {
                    model = DateOnly.ParseExact(value, Constants.DateFormat);
                }
                else
                {
                    throw new NotSupportedException();
                }

                if (model == null && !metadata.IsReferenceOrNullableType)
                {
                    modelState.TryAddModelError(
                        modelName,
                        metadata.ModelBindingMessageProvider.ValueMustNotBeNullAccessor(
                            valueProviderResult.ToString()));
                }
                else
                {
                    bindingContext.Result = ModelBindingResult.Success(model);
                }
            }
            catch (Exception ex)
            {
                modelState.TryAddModelError(modelName, ex, metadata);
            }

            return Task.CompletedTask;
        }
    }
}
