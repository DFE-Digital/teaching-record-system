using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.WebCommon;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class EnableRequestBufferingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        context.HttpContext.Request.EnableBuffering();
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
