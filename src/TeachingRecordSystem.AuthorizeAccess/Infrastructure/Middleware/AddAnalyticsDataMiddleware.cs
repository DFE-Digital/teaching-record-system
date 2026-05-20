using Dfe.Analytics.AspNetCore;
using Dfe.Analytics.Events;
using Microsoft.AspNetCore.Http.Features;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Middleware;

public class AddAnalyticsDataMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IJourneyInstanceProvider journeyInstanceProvider)
    {
        if (context.Features.Get<ISessionFeature>() is not null)
        {
            var coordinator = journeyInstanceProvider.GetSignInJourneyCoordinator(context);

            if (coordinator is not null && context.GetWebRequestEvent() is Event webRequestEvent)
            {
                webRequestEvent.Data[DfeAnalyticsEventDataKeys.ApplicationUserId] = [coordinator.State.ClientApplicationUserId.ToString()];
            }
        }

        await next(context);
    }
}
