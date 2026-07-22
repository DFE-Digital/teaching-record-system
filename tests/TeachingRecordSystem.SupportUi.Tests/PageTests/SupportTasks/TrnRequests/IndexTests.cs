using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TrnRequests;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoOpenTasks_ShowsNoTasksMessage()
    {
        // Arrange
        await TestData.CreateTrnRequestSupportTaskAsync(configure: t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/trn-requests/");

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
        var supportTask = await TestData.CreateTrnRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/trn-requests/?Search=XXX");

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
        var (supportTask, _, _) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("no-tasks-message"));
        Assert.Null(doc.GetElementByTestId("no-results-message"));

        var resultRow = GetResultRows(doc).FirstOrDefault();
        Assert.NotNull(resultRow);

        var nameColumn = resultRow.GetElementByTestId("name");
        Assert.NotNull(nameColumn);
        Assert.Contains($"{supportTask.TrnRequestMetadata!.FirstName} {supportTask.TrnRequestMetadata!.MiddleName} {supportTask.TrnRequestMetadata!.LastName}", nameColumn.TrimmedText());
        Assert.Contains(supportTask.SupportTaskReference, nameColumn.TrimmedText());

        AssertRowHasContent("email", supportTask.TrnRequestMetadata!.EmailAddress ?? string.Empty);
        AssertRowHasContent("requested-on", supportTask.CreatedOn.ToString(WebConstants.DateShortDisplayFormat));
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
            ["ST1"] = await TestData.CreateTrnRequestSupportTaskAsync(
                configure: t => t.WithFirstName("Jim").WithMiddleName("Alan").WithLastName("Smith")
                    .WithCreatedOn(new DateTime(2025, 1, 1))),

            ["ST2"] = await TestData.CreateTrnRequestSupportTaskAsync(
                configure: t => t.WithFirstName("Bob").WithMiddleName("Robert").WithLastName("Jones")
                .WithEmailAddress("bob.jones@email.com")
                .WithCreatedOn(new DateTime(2023, 10, 10))),
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/trn-requests/?Search={Uri.EscapeDataString(search)}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(taskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(TrnRequestsSortByOption.Name, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(TrnRequestsSortByOption.Name, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(TrnRequestsSortByOption.Email, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(TrnRequestsSortByOption.Email, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(TrnRequestsSortByOption.RequestedOn, SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(TrnRequestsSortByOption.RequestedOn, SortDirection.Descending, new[] { "ST1", "ST2" })]
    [InlineData(TrnRequestsSortByOption.Source, SortDirection.Ascending, new[] { "ST1", "ST2" })]
    [InlineData(TrnRequestsSortByOption.Source, SortDirection.Descending, new[] { "ST2", "ST1" })]
    public async Task Get_SortBy_ShowsRequestsInCorrectOrder(TrnRequestsSortByOption sortBy, SortDirection sortDirection, string[] taskKeys)
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "Application Z");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "Application A");

        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser2.UserId, t => t
                .WithFirstName("Zavier")
                .WithEmailAddress("zavier@example.com")
                .WithCreatedOn(new DateTime(2025, 1, 1))),

            ["ST2"] = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser1.UserId, t => t
                .WithFirstName("Aaron")
                .WithEmailAddress("aaron@example.com")
                .WithCreatedOn(new DateTime(2023, 10, 10))),
        });

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/trn-requests/?sortBy={sortBy}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(taskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_WithOpenTasks_ShowsSourceFilterWithCountPerApplicationUser()
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "B application");

        await TestData.CreateTrnRequestSupportTaskAsync(applicationUser1.UserId);
        await TestData.CreateTrnRequestSupportTaskAsync(applicationUser1.UserId);
        await TestData.CreateTrnRequestSupportTaskAsync(applicationUser2.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["A application (2)", "B application (1)"], GetSourceFilterLabels(doc));
    }

    [Fact]
    public async Task Get_SourceFilter_IncludesApplicationUsersWithNoOpenTasks()
    {
        // Arrange
        var applicationUserWithOpenTask = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUserWithClosedTaskOnly = await TestData.CreateApplicationUserAsync(name: "B application");

        await TestData.CreateTrnRequestSupportTaskAsync(applicationUserWithOpenTask.UserId);
        await TestData.CreateTrnRequestSupportTaskAsync(
            applicationUserWithClosedTaskOnly.UserId,
            t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["A application (1)", "B application (0)"], GetSourceFilterLabels(doc));
    }

    [Fact]
    public async Task Get_SourceFilter_UsesApplicationUserShortNameWhenSet()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(name: "A long application name", shortName: "Short");

        await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["Short (1)"], GetSourceFilterLabels(doc));
    }

    [Fact]
    public async Task Get_FilterByApplicationUserIds_ShowsOnlyMatchingResults()
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "B application");

        var tasks = SupportTaskLookup.Create(new()
        {
            ["ST1"] = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser1.UserId),
            ["ST2"] = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser2.UserId),
        });

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/trn-requests/?ApplicationUserIds={applicationUser1.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["ST1"], GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_FilterByApplicationUserIds_ChecksSelectedFilterCheckboxes()
    {
        // Arrange
        var applicationUser1 = await TestData.CreateApplicationUserAsync(name: "A application");
        var applicationUser2 = await TestData.CreateApplicationUserAsync(name: "B application");

        await TestData.CreateTrnRequestSupportTaskAsync(applicationUser1.UserId);
        await TestData.CreateTrnRequestSupportTaskAsync(applicationUser2.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/trn-requests/?ApplicationUserIds={applicationUser1.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal([applicationUser1.UserId.ToString()], GetCheckedSourceFilterValues(doc));
    }

    [Fact]
    public async Task Get_NoOpenTasks_DoesNotShowSourceFilter()
    {
        // Arrange
        await TestData.CreateTrnRequestSupportTaskAsync(configure: t => t.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/trn-requests/");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(GetSourceFilterLabels(doc));
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
            .Select(async (int _, CancellationToken _) => await TestData.CreateTrnRequestSupportTaskAsync())
            .ToArrayAsync();

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/trn-requests/?pageNumber={page}");

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

    private static string[] GetSourceFilterLabels(IHtmlDocument document) =>
        document
            .GetElementsByClassName("govuk-checkboxes__item")
            .Select(item => item.GetElementsByClassName("govuk-checkboxes__label").Single().TrimmedText())
            .ToArray();

    private static string[] GetCheckedSourceFilterValues(IHtmlDocument document) =>
        document
            .QuerySelectorAll("input[name='ApplicationUserIds']")
            .Cast<IHtmlInputElement>()
            .Where(input => input.IsChecked)
            .Select(input => input.Value)
            .ToArray();
}

file static class Extensions
{
    public static void AssertResultsContainsTask(this IHtmlDocument document, string supportTaskReference) =>
        Assert.NotNull(document.GetElementByTestId($"task:{supportTaskReference}"));
}
