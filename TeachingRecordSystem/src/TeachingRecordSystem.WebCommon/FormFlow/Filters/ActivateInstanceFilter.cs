using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;

namespace TeachingRecordSystem.WebCommon.FormFlow.Filters;

internal class ActivateInstanceFilter(JourneyInstanceProvider journeyInstanceProvider) : IAsyncResourceFilter, IOrderedFilter
{
    public const int Order = -100;  // Must run before MissingInstanceFilter

    int IOrderedFilter.Order => Order;

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
        var newInstance = await journeyInstanceProvider.CreateInstanceAsync(context, async id => await CreateStateAsync(id));

        if (journeyDescriptor.AppendUniqueKey)
        {
            // Need to redirect back to ourselves with the unique ID appended
            var request = context.HttpContext.Request;
            var qs = QueryHelpers.ParseQuery(request.QueryString.ToString());
            qs[Constants.UniqueKeyQueryParameterName] = newInstance.InstanceId.UniqueKey!;
            var newUrl = QueryHelpers.AddQueryString(request.Path, qs);
            context.Result = new RedirectResult(newUrl);
            return;
        }

        await next();

        async Task<object> CreateStateAsync(JourneyInstanceId instanceId)
        {
            var journeyStateFactoryType = typeof(IJourneyStateFactory<>).MakeGenericType(journeyDescriptor.StateType);
            if (context.HttpContext.RequestServices.GetService(journeyStateFactoryType) is { } journeyStateFactory)
            {
                var wrapperInstance = (FactoryWrapper)Activator.CreateInstance(
                    typeof(FactoryWrapper<>).MakeGenericType(journeyDescriptor.StateType),
                    journeyStateFactory)!;

                var createStateContext = new CreateJourneyStateContext(journeyDescriptor, instanceId, context.HttpContext);
                return await wrapperInstance.CreateAsync(createStateContext);
            }

            return Activator.CreateInstance(journeyDescriptor.StateType)!;
        }
    }

    private abstract class FactoryWrapper
    {
        public abstract Task<object> CreateAsync(CreateJourneyStateContext context);
    }

    private class FactoryWrapper<T>(IJourneyStateFactory<T> factory) : FactoryWrapper
    {
        public override async Task<object> CreateAsync(CreateJourneyStateContext context)
        {
            var state = await factory.CreateAsync(context);
            return state!;
        }
    }
}
