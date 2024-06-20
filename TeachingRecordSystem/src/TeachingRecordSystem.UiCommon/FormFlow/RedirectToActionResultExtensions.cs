using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace TeachingRecordSystem.UiCommon.FormFlow;

public static class ResultExtensions
{
    public static RedirectToActionResult WithJourneyInstanceUniqueKey(
        this RedirectToActionResult result,
        JourneyInstance instance)
    {
        return WithJourneyInstanceUniqueKey(result, instance.InstanceId);
    }

    public static RedirectToActionResult WithJourneyInstanceUniqueKey(
        this RedirectToActionResult result,
        JourneyInstanceId instanceId)
    {
        if (instanceId.UniqueKey == null)
        {
            throw new ArgumentException(
                "Specified instance does not have a unique key.",
                nameof(instanceId));
        }

        result.RouteValues ??= new RouteValueDictionary();
        result.RouteValues[Constants.UniqueKeyQueryParameterName] = instanceId.UniqueKey;

        return result;
    }

    public static RedirectToPageResult WithJourneyInstanceUniqueKey(
        this RedirectToPageResult result,
        JourneyInstance instance)
    {
        return WithJourneyInstanceUniqueKey(result, instance.InstanceId);
    }

    public static RedirectToPageResult WithJourneyInstanceUniqueKey(
        this RedirectToPageResult result,
        JourneyInstanceId instanceId)
    {
        if (instanceId.UniqueKey == null)
        {
            throw new ArgumentException(
                "Specified instance does not have a unique key.",
                nameof(instanceId));
        }

        result.RouteValues ??= new RouteValueDictionary();
        result.RouteValues[Constants.UniqueKeyQueryParameterName] = instanceId.UniqueKey;

        return result;
    }
}
