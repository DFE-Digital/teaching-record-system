using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoOpenTasks_ShowsNoTasksMessage()
    {
        // Arrange
        await TestData.CreateApiTrnRequestSupportTaskAsync(configure: t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/api-trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));
        Assert.Null(doc.GetElementByTestId("results"));
    }

    [Fact]
    public async Task Get_WithTask_ButNotMatchingSearchCriteria_ShowsNoResultsMessage()
    {
        // Arrange
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/api-trn-requests/?Search=XXX");

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
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var (supportTask, _, _) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/api-trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));

        var resultRow = GetResultRows(doc).FirstOrDefault();
        Assert.NotNull(resultRow);

        AssertRowHasContent("name", $"{supportTask.TrnRequestMetadata!.FirstName} {supportTask.TrnRequestMetadata!.MiddleName} {supportTask.TrnRequestMetadata!.LastName}");
        AssertRowHasContent("email", supportTask.TrnRequestMetadata!.EmailAddress ?? string.Empty);
        AssertRowHasContent("requested-on", supportTask.CreatedOn.ToString(UiDefaults.DateOnlyDisplayFormat));
        AssertRowHasContent("source", applicationUser.Name);

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
    [InlineData("bob.jones@email.com", new[] { "ST2" })]
    [InlineData("1 Jan 2025", new[] { "ST1" })]
    [InlineData("10/10/2023", new[] { "ST2" })]
    public async Task Get_Search_ShowsMatchingResult(string search, string[] taskKeys)
    {
        // Arrange
        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(
                configure: t => t.WithFirstName("Jim").WithLastName("Smith")
                    .WithCreatedOn(new DateTime(2025, 1, 1))),

            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(
                configure: t => t.WithEmailAddress("bob.jones@email.com")
                .WithCreatedOn(new DateTime(2023, 10, 10))),
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/api-trn-requests/?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(taskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(ApiTrnRequestsSortByOption.Name, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(ApiTrnRequestsSortByOption.Name, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(ApiTrnRequestsSortByOption.Email, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(ApiTrnRequestsSortByOption.Email, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(ApiTrnRequestsSortByOption.RequestedOn, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(ApiTrnRequestsSortByOption.RequestedOn, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(ApiTrnRequestsSortByOption.Source, SortDirection.Ascending, new[] { "ST1", "ST2" })]
    [InlineData(ApiTrnRequestsSortByOption.Source, SortDirection.Descending, new[] { "ST2", "ST1" })]
    public async Task Get_SortBy_ShowsRequestsInCorrectOrder(ApiTrnRequestsSortByOption sortBy, SortDirection sortDirection, string[] taskKeys)
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "Application Z");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "Application A");

        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser2.UserId, t => t
                .WithFirstName("Zavier")
                .WithEmailAddress("zavier@example.com")
                .WithCreatedOn(new DateTime(2025, 1, 1))),

            ["ST2"] = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser1.UserId, t => t
                .WithFirstName("Aaron")
                .WithEmailAddress("aaron@example.com")
                .WithCreatedOn(new DateTime(2023, 10, 10))),
        });

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?sortBy={sortBy}&sortDirection={sortDirection}");

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
        await Enumerable.Range(1, (pageSize * page) + 1)
            .ToAsyncEnumerable()
            .Select(async (int _, CancellationToken _) => await TestData.CreateApiTrnRequestSupportTaskAsync())
            .ToArrayAsync();

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/api-trn-requests/?pageNumber={page}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(pageSize, GetResultTaskReferences(doc).Length);
    }

    private static IElement[] GetResultRows(IHtmlDocument document) =>
        document
            .GetElementByTestId("results")?
            .GetElementsByClassName("govuk-table__row")
            .ToArray() ?? [];

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
