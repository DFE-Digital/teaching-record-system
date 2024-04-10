using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Api.Infrastructure.Security;

namespace TeachingRecordSystem.Api.Infrastructure.Filters;

public class AddTrnToSentryScopeResourceFilter(IAuthorizationService authorizationService) : IAsyncResourceFilter
{
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        var identityUserAuthorizeResult = await authorizationService.AuthorizeAsync(user, AuthorizationPolicies.IdentityUserWithTrn);

        if (identityUserAuthorizeResult.Succeeded)
        {
            var trn = user.FindFirstValue("trn")!;

            SentrySdk.ConfigureScope(scope => scope.SetTag("trn", trn));
        }

        await next();
    }
}
