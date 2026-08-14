using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries;

public abstract class ChangeHistoryEntryTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<IHtmlElement> GetEntryHtmlAsync(Guid processId, Guid? personId = null, string? contextType = null, string? oneLoginSubject = null, string? supportTaskReference = null)
    {
        var url = $"_change-history-entry/{processId}";
        var query = new List<string>();

        if (personId is not null)
        {
            query.Add($"personId={personId}");
        }

        if (!string.IsNullOrEmpty(contextType))
        {
            query.Add($"contextType={contextType}");
        }

        if (!string.IsNullOrEmpty(oneLoginSubject))
        {
            query.Add($"oneLoginSubject={Uri.EscapeDataString(oneLoginSubject)}");
        }

        if (!string.IsNullOrEmpty(supportTaskReference))
        {
            query.Add($"supportTaskReference={Uri.EscapeDataString(supportTaskReference)}");
        }

        if (query.Count != 0)
        {
            url += "?" + string.Join("&", query);
        }

        var response = await HttpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var doc = await response.GetDocumentAsync();
        return doc.QuerySelector(".moj-timeline__item") as IHtmlElement ?? throw new InvalidOperationException("Element not found.");
    }

    protected void AssertTitle(IHtmlElement entry, string expectedTitle)
    {
        var title = entry.QuerySelector(".moj-timeline__title");
        Assert.Equal(expectedTitle, title?.TextContent);
    }
}
