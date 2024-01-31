using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess;

public static class PageModelExtensions
{
    public static PageResult PageWithErrors(this PageModel pageModel) => new PageResult() { StatusCode = StatusCodes.Status400BadRequest };
}
