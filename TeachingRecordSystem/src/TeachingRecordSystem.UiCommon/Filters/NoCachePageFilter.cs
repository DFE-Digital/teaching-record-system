using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Net.Http.Headers;

namespace TeachingRecordSystem.UiCommon.Filters;

public class NoCachePageFilter : IPageFilter
{
    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var headers = context.HttpContext.Response.Headers;

        headers[HeaderNames.CacheControl] = "no-store";
        headers.AppendCommaSeparatedValues(HeaderNames.CacheControl, "no-cache");
        headers[HeaderNames.Pragma] = "no-cache";
    }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }
}
