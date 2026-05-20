using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.WebCommon.Filters;

public class NotFoundResourceFilter : IResourceFilter
{
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        throw new NotImplementedException();
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        context.Result = new NotFoundResult();
    }
}
