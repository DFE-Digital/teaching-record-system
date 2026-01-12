using Dfe.Analytics;
using Dfe.Analytics.AspNetCore;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Middleware;

public class AddAnalyticsDataMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IJourneyInstanceProvider journeyInstanceProvider)
    {
        var coordinator = journeyInstanceProvider.GetSignInJourneyCoordinator(context);

        if (coordinator is not null && context.GetWebRequestEvent() is Event webRequestEvent)
        {
            webRequestEvent.Data[DfeAnalyticsEventDataKeys.ApplicationUserId] = [coordinator.State.ClientApplicationUserId.ToString()];
        }

        await next(context);
    }
}
