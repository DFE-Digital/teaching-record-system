namespace TeachingRecordSystem.SupportUi;

public static class LinkGeneratorExtensions
{
    extension(LinkGenerator linkGenerator)
    {
        public string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null)
        {
            return NormalizePath(
                linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found."));
        }

        public string GetJourneyPage(string page, JourneyInstanceId journeyInstanceId, string? returnUrl = null)
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
    }

    // Link generation escapes route values far more aggressively than ASP.NET Core does when it reports a
    // request's path: a One Login subject's ':' becomes "%3A" on the way out but stays as-is on the way in.
    // Both forms address the same page, and GovUk.Questions 1.0.4 matches journey steps across the two, so
    // this is about consistency rather than correctness — without it the same URL is spelled two different
    // ways depending on where it came from, in the address bar and in anything that compares URLs.
    private static string NormalizePath(string url)
    {
        var queryIndex = url.IndexOf('?', StringComparison.Ordinal);
        var path = queryIndex == -1 ? url : url[..queryIndex];
        var query = queryIndex == -1 ? "" : url[queryIndex..];

        return PathString.FromUriComponent(path).ToUriComponent() + query;
    }
}
