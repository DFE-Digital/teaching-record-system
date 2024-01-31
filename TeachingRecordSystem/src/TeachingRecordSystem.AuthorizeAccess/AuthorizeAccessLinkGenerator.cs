using Microsoft.AspNetCore.WebUtilities;
using TeachingRecordSystem.FormFlow;

namespace TeachingRecordSystem.AuthorizeAccess;

public class AuthorizeAccessLinkGenerator(LinkGenerator linkGenerator)
{
    public string Start(JourneyInstanceId journeyInstanceId) => Nino(journeyInstanceId);

    public string Nino(JourneyInstanceId journeyInstanceId) => GetRequiredPathByPage("/Nino", journeyInstanceId: journeyInstanceId);

    private string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null, JourneyInstanceId? journeyInstanceId = null)
    {
        var url = linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");

        if (journeyInstanceId?.UniqueKey is string journeyInstanceUniqueKey)
        {
            url = QueryHelpers.AddQueryString(url, Constants.UniqueKeyQueryParameterName, journeyInstanceUniqueKey);
        }

        return url;
    }
}
