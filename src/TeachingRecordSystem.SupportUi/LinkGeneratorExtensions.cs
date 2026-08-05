using Microsoft.AspNetCore.WebUtilities;

namespace TeachingRecordSystem.SupportUi;

public static class LinkGeneratorExtensions
{
    public static string GetRequiredPathByPage(this LinkGenerator linkGenerator, string page, string? handler = null, object? routeValues = null, TeachingRecordSystem.WebCommon.FormFlow.JourneyInstanceId? journeyInstanceId = null)
    {
        var url = NormalizePath(
            linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found."));

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

        var url = linkGenerator.GetPathByPage(page, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");

        return NormalizePath(url);
    }

    // The journey library identifies a step by the URL of the request that reached it, which ASP.NET Core
    // reports with only the characters that are illegal in a path escaped (a One Login subject's ':' stays
    // as-is). Link generation escapes route values far more aggressively (':' becomes "%3A"), so a step
    // pushed from a generated URL would never match the request it redirects to and the browser would
    // bounce between the two forever. Re-encode generated URLs the way the request will arrive.
    private static string NormalizePath(string url)
    {
        var queryIndex = url.IndexOf('?', StringComparison.Ordinal);
        var path = queryIndex == -1 ? url : url[..queryIndex];
        var query = queryIndex == -1 ? "" : url[queryIndex..];

        return PathString.FromUriComponent(path).ToUriComponent() + query;
    }
}
