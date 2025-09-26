using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TeachingRecordSystem.Api.Infrastructure.ModelBinding;

public class FlagsEnumStringListModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelType = bindingContext.ModelMetadata.UnderlyingOrModelType;

        if (!modelType.IsEnum)
        {
            throw new InvalidOperationException($"Cannot bind type: '{modelType.FullName}'.");
        }

        var logger = bindingContext.HttpContext.RequestServices.GetRequiredService<ILogger<FlagsEnumStringListModelBinder>>();

        var list = bindingContext.ValueProvider.GetValue(bindingContext.ModelName).FirstValue;

        if (string.IsNullOrEmpty(list))
        {
            return Task.CompletedTask;
        }

        int result = 0;
        var splitValues = list.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        foreach (var v in splitValues)
        {
            if (!Enum.TryParse(modelType, v, ignoreCase: true, out var parsed))
            {
                logger.LogDebug("Failed to parse '{Value}' as {ModelType}.", v, modelType);

                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            var x = (int)parsed;

            // Expect each value to be a single flag i.e. a power of 2, except 0
            if ((x & x - 1) == 0 && x != 0)
            {
                result |= x;
            }
            else
            {
                logger.LogDebug("Value '{Result}' is not a single flag of {ModelType}.", parsed, modelType);

                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }
        }

        bindingContext.Result = ModelBindingResult.Success(Enum.ToObject(modelType, result));
        return Task.CompletedTask;
    }
}
