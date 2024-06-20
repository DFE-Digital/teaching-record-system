using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;

namespace TeachingRecordSystem.UiCommon.FormFlow.Filters;

internal class ActivateInstanceFilter(JourneyInstanceProvider journeyInstanceProvider) : IAsyncResourceFilter, IOrderedFilter
{
    public const int Order = -100;  // Must run before MissingInstanceFilter

    int IOrderedFilter.Order => -100;

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        var activatesJourneyMarker = context.ActionDescriptor.GetProperty<ActivatesJourneyMarker>();

        if (activatesJourneyMarker is null)
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
        var state = Activator.CreateInstance(journeyDescriptor.StateType)!;
        var newInstance = await journeyInstanceProvider.CreateInstanceAsync(context, state);

        if (journeyDescriptor.AppendUniqueKey)
        {
            // Need to redirect back to ourselves with the unique ID appended
            var currentUrl = context.HttpContext.Request.GetEncodedUrl();
            var newUrl = QueryHelpers.AddQueryString(currentUrl, Constants.UniqueKeyQueryParameterName, newInstance.InstanceId.UniqueKey!);
            context.Result = new RedirectResult(newUrl);
            return;
        }

        await next();
    }
}
