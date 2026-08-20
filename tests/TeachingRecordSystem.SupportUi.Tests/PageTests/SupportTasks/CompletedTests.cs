using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Services.SupportTasks;
using TeachingRecordSystem.SupportUi.Tests.Services.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class CompletedTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoTasks_ShowsNoResultsMessage()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/completed");

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
        await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithStatus(SupportTaskStatus.Open));

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/completed");

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
        var applicationUser = await TestData.CreateApplicationUserAsync(shortName: "Source App");
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            configure: r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Closed)
                .WithSourceApplicationUserId(applicationUser.UserId),
            configurePerson: p => p
                .WithFirstName("Alice")
                .WithMiddleName("The")
                .WithLastName("Apple"));

        await SetCompletedTaskPropertiesAsync(supportTask, user.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/completed");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var row = doc.GetElementByTestId($"task:{supportTask.SupportTaskReference}");
        Assert.NotNull(row);
        Assert.Equal("Alice The Apple", row.GetElementByTestId("subject")!.TrimmedText());
        Assert.Equal(supportTask.SupportTaskReference, row.GetElementByTestId("task-reference")!.TrimmedText());
        Assert.Equal(new DateTime(2025, 1, 25).ToString(WebConstants.DateDisplayFormat), row.GetElementByTestId("completed-on")!.TrimmedText());
        Assert.Equal("Change name request", row.GetElementByTestId("task-type")!.TrimmedText());
        Assert.Equal("Source App", row.GetElementByTestId("source")!.TrimmedText());
        Assert.Equal("Approved", row.GetElementByTestId("outcome")!.TrimmedText());
        Assert.Equal("Reviewer One", row.GetElementByTestId("completed-by")!.TrimmedText());
    }

    [Theory]
    [InlineData(SupportTaskType.ChangeNameRequest, new[] { "ST1" })]
    [InlineData(SupportTaskType.ChangeDateOfBirthRequest, new[] { "ST2" })]
    public async Task Get_FilterByType_ShowsOnlyTasksOfGivenType(SupportTaskType type, string[] expectedTaskKeys)
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithStatus(SupportTaskStatus.Closed)),
        };

        await SetCompletedTaskPropertiesAsync(tasks["ST1"], user.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST2"], user.UserId, new DateTime(2025, 1, 26), SupportTaskOutcome.ChangeDateOfBirthRequest_Approved);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/completed?type={type}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedTaskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_CompletedByFilterOptions_IncludeUsersWhoCompletedTasksWhateverTheirRoleOrStatus()
    {
        // Arrange
        var inactiveRecordManager = await TestData.CreateUserAsync(active: false, name: "Inactive Record Manager", role: UserRoles.RecordManager);
        var viewer = await TestData.CreateUserAsync(name: "Viewer", role: UserRoles.Viewer);
        var recordManagerWithoutCompletedTasks = await TestData.CreateUserAsync(name: "Record Manager", role: UserRoles.RecordManager);

        var completedByInactiveRecordManager = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
            .WithStatus(SupportTaskStatus.Closed));
        await SetCompletedTaskPropertiesAsync(
            completedByInactiveRecordManager,
            inactiveRecordManager.UserId,
            new DateTime(2025, 1, 25),
            SupportTaskOutcome.ChangeNameRequest_Approved);

        var completedByViewer = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
            .WithStatus(SupportTaskStatus.Closed));
        await SetCompletedTaskPropertiesAsync(
            completedByViewer,
            viewer.UserId,
            new DateTime(2025, 1, 26),
            SupportTaskOutcome.ChangeNameRequest_Approved);

        // Having a task assigned isn't enough on its own
        await AssignToUserAsync(await TestData.CreateChangeNameRequestSupportTaskAsync(), recordManagerWithoutCompletedTasks.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/completed");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        // There's a hidden input with the same id earlier in the page, so select the element by tag name too
        var optionValues = ((IHtmlSelectElement)doc.QuerySelector("select#CompletedByUserId")!)
            .Options
            .Select(o => o.Value)
            .ToArray();

        Assert.Contains(inactiveRecordManager.UserId.ToString(), optionValues);
        Assert.Contains(viewer.UserId.ToString(), optionValues);
        Assert.DoesNotContain(recordManagerWithoutCompletedTasks.UserId.ToString(), optionValues);
    }

    [Fact]
    public async Task Get_FilterByCompletedByUserId_ShowsOnlyTasksCompletedByGivenUser()
    {
        // Arrange
        var userA = await TestData.CreateUserAsync(name: "User A");
        var userB = await TestData.CreateUserAsync(name: "User B");
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 22))
                .WithStatus(SupportTaskStatus.Closed)),
        };

        await SetCompletedTaskPropertiesAsync(tasks["ST1"], userA.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST2"], userB.UserId, new DateTime(2025, 1, 26), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST3"], userA.UserId, new DateTime(2025, 1, 27), SupportTaskOutcome.ChangeNameRequest_Approved);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/completed?completedByUserId={userA.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["ST3", "ST1"], GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(CompletedTasksSortByOption.CompletedOn, SortDirection.Ascending, new[] { "ST1", "ST2", "ST3" })]
    [InlineData(CompletedTasksSortByOption.CompletedOn, SortDirection.Descending, new[] { "ST3", "ST2", "ST1" })]
    public async Task Get_SortByCompletedOn_ShowsTasksInCompletedOnOrder(CompletedTasksSortByOption sortBy, SortDirection sortDirection, string[] expectedTaskKeys)
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 22))
                .WithStatus(SupportTaskStatus.Closed)),
        };

        await SetCompletedTaskPropertiesAsync(tasks["ST1"], user.UserId, new DateTime(2025, 1, 25, 8, 10, 0), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST2"], user.UserId, new DateTime(2025, 1, 25, 12, 30, 0), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST3"], user.UserId, new DateTime(2025, 1, 26, 8, 10, 0), SupportTaskOutcome.ChangeNameRequest_Approved);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/completed?sortBy={sortBy}&sortDirection={sortDirection}");

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
        var user = await TestData.CreateUserAsync();
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(
                configure: r => r
                    .WithCreatedOn(new DateTime(2025, 1, 20))
                    .WithStatus(SupportTaskStatus.Closed),
                configurePerson: p => p.WithFirstName("Zeta").WithMiddleName("The").WithLastName("Zebra")),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(
                configure: r => r
                    .WithCreatedOn(new DateTime(2025, 1, 21))
                    .WithStatus(SupportTaskStatus.Closed),
                configurePerson: p => p.WithFirstName("Alpha").WithMiddleName("The").WithLastName("Ant")),
        };

        await SetCompletedTaskPropertiesAsync(tasks["ST1"], user.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST2"], user.UserId, new DateTime(2025, 1, 26), SupportTaskOutcome.ChangeNameRequest_Approved);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/completed?sortBy={CompletedTasksSortByOption.Subject}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedTaskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(SortDirection.Descending, new[] { "ST1", "ST2" })]
    public async Task Get_SortByTaskType_ShowsTasksInTaskTypeOrder(SortDirection sortDirection, string[] expectedTaskKeys)
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST2"] = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithStatus(SupportTaskStatus.Closed)),
        };

        await SetCompletedTaskPropertiesAsync(tasks["ST1"], user.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST2"], user.UserId, new DateTime(2025, 1, 26), SupportTaskOutcome.ChangeDateOfBirthRequest_Approved);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/completed?sortBy={CompletedTasksSortByOption.TaskType}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedTaskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(SortDirection.Ascending, new[] { "ST1", "ST3", "ST2" })]
    [InlineData(SortDirection.Descending, new[] { "ST2", "ST3", "ST1" })]
    public async Task Get_SortByOutcome_ShowsTasksInOutcomeOrder(SortDirection sortDirection, string[] expectedTaskKeys)
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 22))
                .WithStatus(SupportTaskStatus.Closed)),
        };

        await SetCompletedTaskPropertiesAsync(tasks["ST1"], user.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST2"], user.UserId, new DateTime(2025, 1, 26), SupportTaskOutcome.ChangeNameRequest_Rejected);
        await SetCompletedTaskPropertiesAsync(tasks["ST3"], user.UserId, new DateTime(2025, 1, 27), SupportTaskOutcome.ChangeNameRequest_Cancelled);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/completed?sortBy={CompletedTasksSortByOption.Outcome}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedTaskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Theory]
    [InlineData(SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(SortDirection.Descending, new[] { "ST1", "ST2" })]
    public async Task Get_SortBySource_ShowsTasksInSourceApplicationShortNameOrder(SortDirection sortDirection, string[] expectedTaskKeys)
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var zebraApplicationUser = await TestData.CreateApplicationUserAsync(name: "Ant application", shortName: "Zebra");
        var antApplicationUser = await TestData.CreateApplicationUserAsync(name: "Zebra application", shortName: "Ant");
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Closed)
                .WithSourceApplicationUserId(zebraApplicationUser.UserId)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithStatus(SupportTaskStatus.Closed)
                .WithSourceApplicationUserId(antApplicationUser.UserId)),
        };

        await SetCompletedTaskPropertiesAsync(tasks["ST1"], user.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST2"], user.UserId, new DateTime(2025, 1, 26), SupportTaskOutcome.ChangeNameRequest_Approved);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/completed?sortBy={CompletedTasksSortByOption.Source}&sortDirection={sortDirection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedTaskKeys, GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_SearchBySupportTaskReference_ShowsTaskWithMatchingReference()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithStatus(SupportTaskStatus.Closed)),
        };

        await SetCompletedTaskPropertiesAsync(tasks["ST1"], user.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST2"], user.UserId, new DateTime(2025, 1, 26), SupportTaskOutcome.ChangeNameRequest_Approved);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/completed?search={tasks["ST1"].SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["ST1"], GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_SearchByDate_ShowsTasksCompletedOnThatDate()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithStatus(SupportTaskStatus.Closed)),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 22))
                .WithStatus(SupportTaskStatus.Closed)),
        };

        await SetCompletedTaskPropertiesAsync(tasks["ST1"], user.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST2"], user.UserId, new DateTime(2025, 1, 26), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST3"], user.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/completed?search=25/1/2025");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["ST1", "ST3"], GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_SearchByName_ShowsTasksWithMatchingSubjectName()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(
                configure: r => r
                    .WithCreatedOn(new DateTime(2025, 1, 20))
                    .WithStatus(SupportTaskStatus.Closed),
                configurePerson: p => p.WithFirstName("Alice").WithMiddleName("The").WithLastName("Apple")),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(
                configure: r => r
                    .WithCreatedOn(new DateTime(2025, 1, 21))
                    .WithStatus(SupportTaskStatus.Closed),
                configurePerson: p => p.WithFirstName("Bob").WithMiddleName("The").WithLastName("Builder")),
        };

        await SetCompletedTaskPropertiesAsync(tasks["ST1"], user.UserId, new DateTime(2025, 1, 25), SupportTaskOutcome.ChangeNameRequest_Approved);
        await SetCompletedTaskPropertiesAsync(tasks["ST2"], user.UserId, new DateTime(2025, 1, 26), SupportTaskOutcome.ChangeNameRequest_Approved);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/completed?search=Alice");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["ST1"], GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_ShowsPageOfResults()
    {
        // Arrange
        const int pageSize = 20;
        var user = await TestData.CreateUserAsync();

        // Create enough tasks to spill onto a second page
        for (var i = 0; i < pageSize + 1; i++)
        {
            var task = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithStatus(SupportTaskStatus.Closed));
            await SetCompletedTaskPropertiesAsync(task, user.UserId, DateTime.UtcNow, SupportTaskOutcome.ChangeNameRequest_Approved);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/completed");

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
        var user = await TestData.CreateUserAsync();

        for (var i = 0; i < pageSize + 1; i++)
        {
            var task = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithStatus(SupportTaskStatus.Closed));
            await SetCompletedTaskPropertiesAsync(task, user.UserId, DateTime.UtcNow, SupportTaskOutcome.ChangeNameRequest_Approved);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/completed?pageNumber=2");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Single(GetResultTaskReferences(doc));
    }

    private Task AssignToUserAsync(Core.DataStore.Postgres.Models.SupportTask task, Guid userId) =>
        WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(task.SupportTaskReference);
            dbTask!.AssignedToUserId = userId;
            await dbContext.SaveChangesAsync();
        });

    private Task SetCompletedTaskPropertiesAsync(
        Core.DataStore.Postgres.Models.SupportTask task,
        Guid completedByUserId,
        DateTime completedOn,
        SupportTaskOutcome outcome) =>
        WithDbContextAsync(async dbContext =>
        {
            var dbTask = await dbContext.SupportTasks.FindAsync(task.SupportTaskReference);
            dbTask!.CompletedOn = DateTime.SpecifyKind(completedOn, DateTimeKind.Utc);
            dbTask.CompletedByUserId = completedByUserId;
            dbTask.Outcome = outcome;
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

file static class Extensions
{
    public static string GetTextContentWithNormalizedWhitespace(this IElement element)
    {
        var textContent = element.TextContent;
        return Regex.Replace(textContent, @"\s+", " ").Trim();
    }
}
