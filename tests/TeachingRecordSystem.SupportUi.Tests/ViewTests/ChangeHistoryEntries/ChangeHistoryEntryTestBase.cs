using AngleSharp.Html.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.ChangeHistoryEntries;

public abstract class ChangeHistoryEntryTestBase(HostFixture hostFixture) : TestBase(hostFixture)
{
    protected async Task<IHtmlElement> GetEntryHtmlAsync(Guid processId, Guid? personId = null)
    {
        var url = $"_change-history-entry/{processId}" + (personId is not null ? $"?personId={personId}" : "");
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
