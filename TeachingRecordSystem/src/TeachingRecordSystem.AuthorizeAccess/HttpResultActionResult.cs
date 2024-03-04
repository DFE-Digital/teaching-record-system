using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.AuthorizeAccess;

public class HttpResultActionResult(IResult result) : IActionResult
{
    public Task ExecuteResultAsync(ActionContext context)
    {
        return result.ExecuteAsync(context.HttpContext);
    }
}
