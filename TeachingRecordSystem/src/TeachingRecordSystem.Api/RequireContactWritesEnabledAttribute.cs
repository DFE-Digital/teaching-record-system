using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.Api;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireContactWritesEnabledAttribute : Attribute, IFilterFactory
{
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return new RequireContactWritesEnabledFilter(configuration);
    }

    public bool IsReusable => true;
}

public class RequireContactWritesEnabledFilter(IConfiguration configuration) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (configuration["DisableContactWrites"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
