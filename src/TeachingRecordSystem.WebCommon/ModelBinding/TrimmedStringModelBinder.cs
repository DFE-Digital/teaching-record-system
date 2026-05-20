using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TeachingRecordSystem.WebCommon.ModelBinding;

public class TrimmedStringModelBinder(ILoggerFactory loggerFactory) : IModelBinder
{
    private readonly SimpleTypeModelBinder _innerBinder = new(typeof(string), loggerFactory);

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        await _innerBinder.BindModelAsync(bindingContext);

        if (bindingContext.Result is { IsModelSet: true, Model: string strValue })
        {
            bindingContext.Result = ModelBindingResult.Success(strValue.Trim());
        }
    }
}

public class TrimmedStringModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(string))
        {
            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            return new TrimmedStringModelBinder(loggerFactory);
        }

        return null;
    }
}
