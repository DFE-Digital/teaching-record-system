using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class RequireClosedAlertFilter : IResourceFilter, IOrderedFilter
{
    public int Order => FilterOrders.RequireClosedAlertFilterOrder;  // After CheckAlertExistsFilter

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var alertFeature = context.HttpContext.Features.GetRequiredFeature<CurrentAlertFeature>();

        if (alertFeature.Alert.IsOpen)
        {
            context.Result = new BadRequestResult();
        }
    }
}
