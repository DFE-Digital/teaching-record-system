using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.Api.Infrastructure.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class SupportsReadOnlyModeAttribute : Attribute, IActionModelConvention, IControllerModelConvention
{
    public void Apply(ActionModel action)
    {
        action.Properties.Add(typeof(SupportsReadOnlyModeMarker), SupportsReadOnlyModeMarker.Instance);
    }

    public void Apply(ControllerModel controller)
    {
        controller.Properties.Add(typeof(SupportsReadOnlyModeMarker), SupportsReadOnlyModeMarker.Instance);
    }
}

public class SupportsReadOnlyModeMarker
{
    private SupportsReadOnlyModeMarker() { }

    public static SupportsReadOnlyModeMarker Instance { get; } = new();
}

public class ReadOnlyModeFilterFactory : IFilterFactory, IOrderedFilter
{
    public bool IsReusable => true;

    public int Order => int.MinValue;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<ReadOnlyModeFilter>();
    }
}

public class ReadOnlyModeFilter : IActionFilter
{
    private readonly IConfiguration _configuration;

    public ReadOnlyModeFilter(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (_configuration.GetValue<bool>("ReadOnlyMode"))
        {
            var supportsReadOnlyModeMarker = context.ActionDescriptor.GetProperty<SupportsReadOnlyModeMarker>();

            if (supportsReadOnlyModeMarker is null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
