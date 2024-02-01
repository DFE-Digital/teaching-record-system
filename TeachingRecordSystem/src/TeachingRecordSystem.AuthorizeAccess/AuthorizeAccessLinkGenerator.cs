using Microsoft.AspNetCore.WebUtilities;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess;

public abstract class AuthorizeAccessLinkGenerator
{
    public string DebugIdentity(JourneyInstanceId journeyInstanceId) => GetRequiredPathByPage("/DebugIdentity", journeyInstanceId: journeyInstanceId);

    public string Nino(JourneyInstanceId journeyInstanceId) => GetRequiredPathByPage("/Nino", journeyInstanceId: journeyInstanceId);

    protected virtual string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null, JourneyInstanceId? journeyInstanceId = null)
    {
        var url = GetRequiredPathByPage(page, handler, routeValues);

        if (journeyInstanceId?.UniqueKey is string journeyInstanceUniqueKey)
        {
            url = QueryHelpers.AddQueryString(url, Constants.UniqueKeyQueryParameterName, journeyInstanceUniqueKey);
        }

        return url;
    }

    protected abstract string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null);
}

public class RoutingAuthorizeAccessLinkGenerator(LinkGenerator linkGenerator) : AuthorizeAccessLinkGenerator
{
    protected override string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null) =>
        linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");
}
