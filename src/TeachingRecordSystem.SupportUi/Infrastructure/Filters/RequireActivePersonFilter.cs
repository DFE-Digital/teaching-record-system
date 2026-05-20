using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class RequireActivePersonFilter : IResourceFilter, IOrderedFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var currentPerson = context.HttpContext.Features.Get<CurrentPersonFeature>();

        if (currentPerson is null)
        {
            return;
        }

        if (context.ActionDescriptor.EndpointMetadata.Any(m => m is AllowDeactivatedPersonMetadata))
        {
            return;
        }

        if (currentPerson.Status is not PersonStatus.Active)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }

    public int Order => 100;
}
