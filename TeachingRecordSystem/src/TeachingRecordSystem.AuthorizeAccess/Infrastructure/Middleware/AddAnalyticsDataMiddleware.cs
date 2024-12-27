using Dfe.Analytics;
using Dfe.Analytics.AspNetCore;
using TeachingRecordSystem.AuthorizeAccess.Infrastructure.FormFlow;
using TeachingRecordSystem.WebCommon.FormFlow.State;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Middleware;

public class AddAnalyticsDataMiddleware(IUserInstanceStateProvider userInstanceStateProvider, RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var journeyInstance = await userInstanceStateProvider.GetSignInJourneyInstanceAsync(context);
        if (journeyInstance is not null && context.GetWebRequestEvent() is Event webRequestEvent)
        {
            webRequestEvent.Data[DfeAnalyticsEventDataKeys.ApplicationUserId] = [journeyInstance.State.ClientApplicationUserId.ToString()];
        }

        await next(context);
    }
}
