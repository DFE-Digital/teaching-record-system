using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class CheckUserExistsFilter : IAsyncResourceFilter, IOrderedFilter
{
    public int Order => int.MinValue;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated == true &&
            (!user.Claims.Any(c => c.Type == CustomClaims.UserId) || !user.Claims.Any(c => c.Type == ClaimTypes.Role)))
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
