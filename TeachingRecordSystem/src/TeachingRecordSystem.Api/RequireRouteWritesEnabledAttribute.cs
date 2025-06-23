using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.Api;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireRouteWritesEnabledAttribute : Attribute, IFilterFactory
{
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return new RequireRouteWritesEnabledFilter(configuration);
    }

    public bool IsReusable => true;
}

public class RequireRouteWritesEnabledFilter(IConfiguration configuration) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (configuration["DisableRouteWrites"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
