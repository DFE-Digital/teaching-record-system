using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class CheckUserExistsFilter : IAsyncResourceFilter, IOrderedFilter
{
    public int Order => int.MinValue;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated == true && !user.IsActiveTrsUser())
        {
            var viewResult = new ViewResult()
            {
                StatusCode = StatusCodes.Status403Forbidden,
                ViewName = "NoRoles"
            };

            context.Result = viewResult;
        }
        else
        {
            await next();
        }
    }
}
