using GovUk.Frontend.AspNetCore.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;

public class DateOnlyModelBinder : IModelBinder
{
    public const string Format = "yyyy-MM-dd";

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (!string.IsNullOrEmpty(value.FirstValue))
        {
            if (DateOnly.TryParseExact(value.FirstValue, Format, out var result))
            {
                bindingContext.Result = ModelBindingResult.Success(result);
            }
        }

        return Task.CompletedTask;
    }
}

public class DateOnlyModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.UnderlyingOrModelType == typeof(DateOnly) &&
            context.Metadata.BindingSource == BindingSource.Query)
        {
            return new DateOnlyModelBinder();
        }

        return null;
    }
}
