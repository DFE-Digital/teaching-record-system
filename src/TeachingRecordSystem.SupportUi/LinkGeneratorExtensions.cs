using Microsoft.AspNetCore.WebUtilities;

namespace TeachingRecordSystem.SupportUi;

public static class LinkGeneratorExtensions
{
    public static string GetRequiredPathByPage(this LinkGenerator linkGenerator, string page, string? handler = null, object? routeValues = null, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null)
    {
        var url = linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");

        if (journeyInstanceId?.UniqueKey is string journeyInstanceUniqueKey)
        {
            url = QueryHelpers.AddQueryString(url, WebCommon.FormFlow.Constants.UniqueKeyQueryParameterName, journeyInstanceUniqueKey);
        }

        return url;
    }

    public static string GetJourneyPage(this LinkGenerator linkGenerator, string page, JourneyInstanceId journeyInstanceId, string? returnUrl = null)
    {
        var routeValues = new RouteValueDictionary();

        // Add the scoping route values (e.g. personId) before the instance key so that generated
        // URLs read personId first, then returnUrl, then _jid.
        foreach (var kvp in journeyInstanceId.RouteValues.Where(kvp => kvp.Key != JourneyInstanceId.KeyRouteValueName))
        {
            routeValues[kvp.Key] = kvp.Value;
        }

        if (returnUrl is not null)
        {
            routeValues[JourneyCoordinator.ReturnUrlQueryParameterName] = returnUrl;
        }

        routeValues[JourneyInstanceId.KeyRouteValueName] = journeyInstanceId.Key;

        return linkGenerator.GetPathByPage(page, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");
    }
}
