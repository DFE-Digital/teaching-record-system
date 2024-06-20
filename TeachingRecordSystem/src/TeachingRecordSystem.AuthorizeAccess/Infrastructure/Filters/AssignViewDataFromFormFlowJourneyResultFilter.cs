using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;
using TeachingRecordSystem.UiCommon.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Filters;

public class AssignViewDataFromFormFlowJourneyResultFilter(IUserInstanceStateProvider stateProvider, AuthorizeAccessLinkGenerator linkGenerator) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var journeyInstance = await stateProvider.GetSignInJourneyInstanceAsync(context.HttpContext);

        if (journeyInstance is not null && context.Result is PageResult pageResult)
        {
            pageResult.ViewData.Add("ServiceName", journeyInstance.State.ServiceName);
            pageResult.ViewData.Add("ServiceUrl", journeyInstance.State.ServiceUrl);
            pageResult.ViewData.Add("SignOutLink", linkGenerator.SignOut(journeyInstance.InstanceId));
        }

        await next();
    }
}

public class AssignViewDataFromFormFlowJourneyResultFilterFactory : IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider) =>
        ActivatorUtilities.CreateInstance<AssignViewDataFromFormFlowJourneyResultFilter>(serviceProvider);
}
