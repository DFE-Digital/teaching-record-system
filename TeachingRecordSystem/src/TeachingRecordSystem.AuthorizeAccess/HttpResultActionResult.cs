using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.AuthorizeAccess;

public class HttpResultActionResult(IResult result) : IActionResult
{
    public Task ExecuteResultAsync(ActionContext context)
    {
        return result.ExecuteAsync(context.HttpContext);
    }
}

public static class ResultExtensions
{
    public static IActionResult ToActionResult(this IResult result) => new HttpResultActionResult(result);
}
