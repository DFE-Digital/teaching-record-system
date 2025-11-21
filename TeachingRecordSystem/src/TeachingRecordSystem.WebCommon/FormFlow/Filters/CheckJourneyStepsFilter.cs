using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.WebCommon.FormFlow.Filters;

public class CheckJourneyStepsFilter(JourneyInstanceProvider journeyInstanceProvider) : IAsyncResourceFilter, IOrderedFilter
{
    public const int Order = -10;

    int IOrderedFilter.Order => Order;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var instance = await journeyInstanceProvider.ResolveCurrentInstanceAsync(context);

        if (instance?.State is IJourneyWithSteps { Steps: { } journeySteps })
        {
            var currentStep = context.HttpContext.CreateStepForCurrentRequest();

            if (!journeySteps.ContainsStep(currentStep))
            {
                context.Result = new RedirectResult(journeySteps.LastStepUrl);
                return;
            }
        }

        await next();
    }
}
