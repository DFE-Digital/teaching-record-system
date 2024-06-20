using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace TeachingRecordSystem.UiCommon.FormFlow.Filters;

internal class MissingInstanceFilter(IOptions<FormFlowOptions> optionsAccessor, JourneyInstanceProvider journeyInstanceProvider) : IAsyncResourceFilter, IOrderedFilter
{
    public const int Order = 0;  // Must run after ActivateInstanceFilter

    int IOrderedFilter.Order => 0;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var requireInstanceMarker = context.ActionDescriptor.GetProperty<RequireInstanceMarker>();

        if (requireInstanceMarker is null)
        {
            await next();
            return;
        }

        var instance = await journeyInstanceProvider.ResolveCurrentInstanceAsync(context);
        if (instance is not null)
        {
            await next();
            return;
        }

        var journeyDescriptor = journeyInstanceProvider.ResolveJourneyDescriptor(context, throwIfNotFound: true)!;
        context.Result = optionsAccessor.Value.MissingInstanceHandler(journeyDescriptor, context.HttpContext, requireInstanceMarker.ErrorStatusCode);
    }
}
