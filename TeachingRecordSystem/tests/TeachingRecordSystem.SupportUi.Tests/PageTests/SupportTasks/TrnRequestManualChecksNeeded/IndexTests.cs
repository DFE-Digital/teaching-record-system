using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TrnRequestManualChecksNeeded;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoOpenTasks_ShowsNoTasksMessage()
    {
        // Arrange
        await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(status: SupportTaskStatus.Closed);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/manual-checks-needed");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));
        Assert.Null(doc.GetElementByTestId("results"));
    }

    [Fact]
    public async Task Get_WithSupportTask_ButNotMatchingSearchCriteria_ShowsNoResultsMessage()
    {
        // Arrange
        await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/manual-checks-needed/?Search=XXX");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("no-tasks-message"));
        Assert.NotNull(doc.GetElementByTestId("no-results-message"));
        Assert.Null(doc.GetElementByTestId("results"));
    }

    [Fact]
    public async Task Get_WithTask_ShowsExpectedDataInResultsTable()
    {
        // Arrange
        var supportTask = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/manual-checks-needed");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));

        var resultRow = GetResultRows(doc).FirstOrDefault();
        Assert.NotNull(resultRow);

        AssertRowHasContent("name", $"{supportTask.TrnRequestMetadata!.FirstName} {supportTask.TrnRequestMetadata!.MiddleName} {supportTask.TrnRequestMetadata!.LastName}");
        AssertRowHasContent("created-on", supportTask.CreatedOn.ToString(WebConstants.DateOnlyDisplayFormat));
        AssertRowHasContent("date-of-birth", supportTask.TrnRequestMetadata!.DateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat));
        AssertRowHasContent("source", supportTask.TrnRequestMetadata.ApplicationUser!.Name);

        void AssertRowHasContent(string testId, string expectedText)
        {
            var column = resultRow.GetElementByTestId(testId);
            Assert.NotNull(column);
            Assert.Equal(expectedText, column.TrimmedText());
        }
    }

    [Theory]
    [InlineData("Smith", new[] { "ST1" })]
    [InlineData("Jim", new[] { "ST1" })]
    [InlineData("Jim Smith", new[] { "ST1" })]
    public async Task Get_Search_ShowsMatchingResult(string search, string[] taskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: t => t.WithFirstName("Jim").WithLastName("Smith")),

            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(
                configureApiTrnRequest: t => t.WithFirstName("Bob").WithLastName("Jones")),
        };

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/manual-checks-needed/?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(taskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(TrnRequestManualChecksSortByOption.Name, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(TrnRequestManualChecksSortByOption.Name, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(TrnRequestManualChecksSortByOption.DateOfBirth, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(TrnRequestManualChecksSortByOption.DateOfBirth, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(TrnRequestManualChecksSortByOption.DateCreated, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(TrnRequestManualChecksSortByOption.DateCreated, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(TrnRequestManualChecksSortByOption.Source, SortDirection.Ascending, new[] { "ST1", "ST2" })]
    [InlineData(TrnRequestManualChecksSortByOption.Source, SortDirection.Descending, new[] { "ST2", "ST1" })]
    public async Task Get_SortBy_ShowsRequestsInCorrectOrder(TrnRequestManualChecksSortByOption sortBy, SortDirection sortDirection, string[] taskKeys)
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "Application Z");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "Application A");

        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser2.UserId,
                createdOn: new DateTime(2025, 1, 1),
                configureApiTrnRequest: t => t
                    .WithFirstName("Zavier")
                    .WithDateOfBirth(new(2025, 1, 1))),

            ["ST2"] = await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync(applicationUser1.UserId,
                createdOn: new DateTime(2023, 10, 10),
                configureApiTrnRequest: t => t
                    .WithFirstName("Aaron")
                    .WithDateOfBirth(new(2023, 10, 10)))
        };

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/manual-checks-needed/?sortBy={sortBy}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(taskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_ShowsPageOfResults()
    {
        // Arrange
        var pageSize = 20;
        var page = 2;

        // Create enough tasks to create 3 pages
        var tasks = await AsyncEnumerable.ToArrayAsync(Enumerable.Range(1, (pageSize * page) + 1)
            .ToAsyncEnumerable()
            .SelectAwait(async _ => await TestData.CreateTrnRequestManualChecksNeededSupportTaskAsync()));

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/manual-checks-needed?pageNumber={page}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(pageSize, GetResultTaskReferences(doc).Length);
    }

    private static IElement[] GetResultRows(IHtmlDocument document)
    {
        var r =
        document
            .GetElementByTestId("results");

        return r?
            .GetElementsByClassName("govuk-table__row")
            .ToArray() ?? [];
    }

    private static string[] GetResultTaskReferences(IHtmlDocument document) =>
        GetResultRows(document)
            .Select(row => row.GetAttribute("data-testid")!["task:".Length..])
            .ToArray();

    private static string[] GetResultTaskKeys(IHtmlDocument document, SupportTaskLookup tasks) =>
        GetResultTaskReferences(document)
            .Select(tasks.GetKeyFor)
            .ToArray();
}

file static class Extensions
{
    public static void AssertResultsContainsTask(this IHtmlDocument document, string supportTaskReference) =>
        Assert.NotNull(document.GetElementByTestId($"task:{supportTaskReference}"));
}
