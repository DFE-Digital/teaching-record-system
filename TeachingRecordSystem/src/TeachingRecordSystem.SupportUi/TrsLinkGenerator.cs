using Microsoft.AspNetCore.WebUtilities;
using TeachingRecordSystem.SupportUi.Infrastructure.ModelBinding;

namespace TeachingRecordSystem.SupportUi;

public partial class TrsLinkGenerator(LinkGenerator linkGenerator)
{
    protected const string DateOnlyFormat = DateOnlyModelBinder.Format;

    public string Index() => GetRequiredPathByPage("/Index");

    public string SignOut() => GetRequiredPathByPage("/SignOut");

    public string SignedOut() => GetRequiredPathByPage("/SignedOut");

    private string GetRequiredPathByPage(string page, string? handler = null, object? routeValues = null, JourneyInstanceId? journeyInstanceId = null)
    {
        var url = linkGenerator.GetPathByPage(page, handler, values: routeValues) ?? throw new InvalidOperationException("Page was not found.");

        if (journeyInstanceId?.UniqueKey is string journeyInstanceUniqueKey)
        {
            url = QueryHelpers.AddQueryString(url, WebCommon.FormFlow.Constants.UniqueKeyQueryParameterName, journeyInstanceUniqueKey);
        }

        return url;
    }
}
