using AngleSharp.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.ViewTests.SortableTables;

public class SortableTableTagHelperTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task SortableColumn_MovesHtmxAttributesFromHeaderToButton()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/_sortable-table");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var header = await GetSortableHeaderAsync(response);

        Assert.DoesNotContain(header.Attributes, a => a.Name.StartsWith("hx-"));

        var button = header.QuerySelector("button");
        Assert.NotNull(button);
        Assert.Equal("main", button.GetAttribute("hx-select"));
        Assert.Equal("/_sortable-table?sortDirection=Ascending", button.GetAttribute("hx-get"));
    }

    [Fact]
    public async Task SortableColumn_HtmxAttributeValueNeedingEncoding_IsEncodedOnlyOnce()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/_sortable-table");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var header = await GetSortableHeaderAsync(response);

        var button = header.QuerySelector("button");
        Assert.NotNull(button);
        Assert.Equal("[name='SupportTaskReference']", button.GetAttribute("hx-include"));
    }

    private static async Task<IElement> GetSortableHeaderAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        var doc = await response.GetDocumentAsync();
        return doc.QuerySelector("th[aria-sort]") ?? throw new InvalidOperationException("Sortable column header not found.");
    }
}
