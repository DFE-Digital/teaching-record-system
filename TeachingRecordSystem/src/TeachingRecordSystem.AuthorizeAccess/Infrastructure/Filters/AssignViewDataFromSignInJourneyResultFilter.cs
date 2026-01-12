using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Filters;

public class AssignViewDataFromSignInJourneyResultFilter(
    IJourneyInstanceProvider journeyInstanceProvider,
    AuthorizeAccessLinkGenerator linkGenerator) :
    IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        var coordinator = journeyInstanceProvider.GetJourneyInstance(context.HttpContext) as SignInJourneyCoordinator;

        if (coordinator is not null && context.Result is PageResult pageResult)
        {
            pageResult.ViewData.Add("ServiceName", coordinator.State.ServiceName);
            pageResult.ViewData.Add("ServiceUrl", coordinator.State.ServiceUrl);
            pageResult.ViewData.Add("SignOutLink", linkGenerator.SignOut(coordinator.InstanceId));
        }
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}

public class AssignViewDataFromFormFlowJourneyResultFilterFactory : IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<AssignViewDataFromSignInJourneyResultFilter>(serviceProvider);
}
