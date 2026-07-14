using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class ActiveTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoTasks_ShowsNoResultsMessage()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementByTestId("no-results-message"));
        Assert.Null(doc.GetElementByTestId("results"));
    }

    [Fact]
    public async Task Get_NoTasksMatchingFilters_ShowsNoResultsMessage()
    {
        // Arrange
        await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithStatus(SupportTaskStatus.Closed));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementByTestId("no-results-message"));
        Assert.Null(doc.GetElementByTestId("results"));
    }

    [Fact]
    public async Task Get_WithTask_ShowsExpectedDataInResultsTable()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(name: "Reviewer One");
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            configure: r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.InProgress),
            configurePerson: p => p
                .WithFirstName("Alice")
                .WithMiddleName("The")
                .WithLastName("Apple"));
        await AssignToUserAsync(supportTask, user.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var row = doc.GetElementByTestId($"task:{supportTask.SupportTaskReference}");
        Assert.NotNull(row);
        Assert.Equal("Alice The Apple", row.GetElementByTestId("task-name")!.TrimmedText());
        Assert.Equal(supportTask.SupportTaskReference, row.GetElementByTestId("task-reference")!.TrimmedText());
        Assert.Equal(new DateTime(2025, 1, 20).ToString(WebConstants.DateDisplayFormat), row.GetElementByTestId("requested-on")!.TrimmedText());
        Assert.Equal("In progress", row.GetElementByTestId("status")!.TrimmedText());
        Assert.Equal("Reviewer One", row.GetElementByTestId("assigned-to")!.TrimmedText());
    }

    [Fact]
    public async Task Get_UnassignedTask_ShowsNotAssigned()
    {
        // Arrange
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var row = doc.GetElementByTestId($"task:{supportTask.SupportTaskReference}");
        Assert.NotNull(row);
        Assert.Equal("Not assigned", row.GetElementByTestId("assigned-to")!.TrimmedText());
    }

    [Fact]
    public async Task Get_NoStatusFilter_ShowsOnlyOpenAndInProgressTasks()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)).WithStatus(SupportTaskStatus.Open)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)).WithStatus(SupportTaskStatus.InProgress)),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22)).WithStatus(SupportTaskStatus.Closed)),
        };

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["ST1", "ST2"], GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(SupportTaskStatus.Open, new[] { "ST1" })]
    [InlineData(SupportTaskStatus.InProgress, new[] { "ST2" })]
    [InlineData(SupportTaskStatus.Closed, new[] { "ST3" })]
    public async Task Get_FilterByStatus_ShowsOnlyTasksWithGivenStatus(SupportTaskStatus status, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)).WithStatus(SupportTaskStatus.Open)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)).WithStatus(SupportTaskStatus.InProgress)),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22)).WithStatus(SupportTaskStatus.Closed)),
        };

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/active?status={status}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedTaskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(SupportTaskType.ChangeNameRequest, new[] { "ST1" })]
    [InlineData(SupportTaskType.ChangeDateOfBirthRequest, new[] { "ST2" })]
    public async Task Get_FilterByType_ShowsOnlyTasksOfGivenType(SupportTaskType type, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
        };

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/active?type={type}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedTaskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_FilterByAssignedToUserId_ShowsOnlyTasksAssignedToGivenUser()
    {
        // Arrange
        var userA = await TestData.CreateUserAsync(name: "User A");
        var userB = await TestData.CreateUserAsync(name: "User B");
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22))),
        };
        await AssignToUserAsync(tasks["ST1"], userA.UserId);
        await AssignToUserAsync(tasks["ST2"], userB.UserId);
        await AssignToUserAsync(tasks["ST3"], userA.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/active?assignedToUserId={userA.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["ST1", "ST3"], GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(SortDirection.Ascending, new[] { "ST2", "ST1", "ST3" })]
    [InlineData(SortDirection.Descending, new[] { "ST3", "ST1", "ST2" })]
    public async Task Get_SortByRequestedOn_ShowsTasksInCreatedOnOrder(SortDirection sortDirection, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20, 12, 30, 0))),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20, 8, 10, 0))),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21, 8, 10, 0))),
        };

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/active?sortBy={SupportTasksSortByOption.RequestedOn}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedTaskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(SortDirection.Descending, new[] { "ST1", "ST2" })]
    public async Task Get_SortBySubject_ShowsTasksInSubjectOrder(SortDirection sortDirection, string[] expectedTaskKeys)
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(
                configure: r => r.WithCreatedOn(new DateTime(2025, 1, 20)),
                configurePerson: p => p.WithFirstName("Zeta").WithMiddleName("The").WithLastName("Zebra")),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(
                configure: r => r.WithCreatedOn(new DateTime(2025, 1, 21)),
                configurePerson: p => p.WithFirstName("Alpha").WithMiddleName("The").WithLastName("Ant")),
        };

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/active?sortBy={SupportTasksSortByOption.Subject}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedTaskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_ShowsPageOfResults()
    {
        // Arrange
        const int pageSize = 20;

        // Create enough tasks to spill onto a second page
        for (var i = 0; i < pageSize + 1; i++)
        {
            await TestData.CreateChangeNameRequestSupportTaskAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(pageSize, GetResultTaskReferences(doc).Length);
    }

    [Fact]
    public async Task Get_SecondPage_ShowsRemainingResults()
    {
        // Arrange
        const int pageSize = 20;

        for (var i = 0; i < pageSize + 1; i++)
        {
            await TestData.CreateChangeNameRequestSupportTaskAsync();
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active?pageNumber=2");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Single(GetResultTaskReferences(doc));
    }

    private Task AssignToUserAsync(SupportTask task, Guid userId) =>
        WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(task.SupportTaskReference);
            dbTask!.AssignedToUserId = userId;
            await dbContext.SaveChangesAsync();
        });

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
