using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;

public class MultiLineStringModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelType = bindingContext.ModelType;
        if (!typeof(string[]).IsAssignableTo(modelType))
        {
            throw new InvalidOperationException($"Cannot bind to {modelType}.");
        }

        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        var value = valueProviderResult.FirstValue ?? string.Empty;
        var splitByLines = value.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        bindingContext.Result = ModelBindingResult.Success(splitByLines);

        return Task.CompletedTask;
    }
}
