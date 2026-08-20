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
        var applicationUser = await TestData.CreateApplicationUserAsync(shortName: "Source App");
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
            configure: r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithStatus(SupportTaskStatus.InProgress)
                .WithSourceApplicationUserId(applicationUser.UserId),
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
        Assert.Equal(new DateTime(2025, 1, 20).ToString(WebConstants.DateShortDisplayFormat), row.GetElementByTestId("requested-on")!.TrimmedText());
        Assert.Equal("In progress", row.GetElementByTestId("status")!.TrimmedText());
        Assert.Equal("Reviewer One", row.GetElementByTestId("assigned-to")!.TrimmedText());
        Assert.Equal("Source App", row.GetElementByTestId("source")!.TrimmedText());
    }

    [Fact]
    public async Task Get_UnassignedTask_ShowsUnassigned()
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
        Assert.Equal("Unassigned", row.GetElementByTestId("assigned-to")!.TrimmedText());
    }

    [Fact]
    public async Task Get_TaskAssignedToCurrentUser_ShowsMyself()
    {
        // Arrange
        var currentUser = await CreateAndSetCurrentUserAsync();
        var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync();
        await AssignToUserAsync(supportTask, currentUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var row = doc.GetElementByTestId($"task:{supportTask.SupportTaskReference}");
        Assert.NotNull(row);
        Assert.Equal("Myself", row.GetElementByTestId("assigned-to")!.TrimmedText());
    }

    [Fact]
    public async Task Get_AssignedToFilterOptions_IncludeUnassignedAndMyselfAndExcludeCurrentUserFromUserList()
    {
        // Arrange
        var currentUser = await CreateAndSetCurrentUserAsync(name: "Current User");
        var otherUser = await TestData.CreateUserAsync(name: "Other User", role: UserRoles.RecordManager);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var options = ((IHtmlSelectElement)doc.GetElementById("AssignedToUserId")!)
            .Options
            .Select(o => (o.Value, Text: o.TrimmedText()))
            .ToArray();

        Assert.Equal(
            [
                ("", "All owners"),
                (SupportTaskSearchService.UnassignedUserId.ToString(), "Unassigned"),
                (currentUser.UserId.ToString(), "Myself"),
                (otherUser.UserId.ToString(), "Other User")
            ],
            options);
    }

    [Fact]
    public async Task Get_AssignedToFilterOptions_IncludeUsersWithTasksAssignedWhateverTheirRoleOrStatus()
    {
        // Arrange
        var currentUser = await CreateAndSetCurrentUserAsync(name: "Current User");
        var inactiveRecordManager = await TestData.CreateUserAsync(active: false, name: "Inactive Record Manager", role: UserRoles.RecordManager);
        var viewer = await TestData.CreateUserAsync(name: "Viewer", role: UserRoles.Viewer);
        var viewerWithoutTasks = await TestData.CreateUserAsync(name: "Viewer Without Tasks", role: UserRoles.Viewer);

        await AssignToUserAsync(await TestData.CreateChangeNameRequestSupportTaskAsync(), inactiveRecordManager.UserId);
        await AssignToUserAsync(await TestData.CreateChangeNameRequestSupportTaskAsync(), viewer.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var optionValues = ((IHtmlSelectElement)doc.GetElementById("AssignedToUserId")!)
            .Options
            .Select(o => o.Value)
            .ToArray();

        Assert.Contains(inactiveRecordManager.UserId.ToString(), optionValues);
        Assert.Contains(viewer.UserId.ToString(), optionValues);
        Assert.DoesNotContain(viewerWithoutTasks.UserId.ToString(), optionValues);
        Assert.Contains(currentUser.UserId.ToString(), optionValues);
    }

    [Fact]
    public async Task Get_CurrentUserIsNotAssignable_AssignedToFilterOptionsDoNotIncludeMyself()
    {
        // Arrange
        SupportTaskAssignmentOptions.IncludeAdministrators = false;

        var currentUser = await TestData.CreateUserAsync(name: "Current User", role: UserRoles.Administrator);
        SetCurrentUser(currentUser);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var options = ((IHtmlSelectElement)doc.GetElementById("AssignedToUserId")!)
            .Options
            .Select(o => (o.Value, Text: o.TrimmedText()))
            .ToArray();

        Assert.DoesNotContain(options, o => o.Text == "Myself");
        Assert.DoesNotContain(options, o => o.Value == currentUser.UserId.ToString());
    }

    [Fact]
    public async Task Get_IncludeAdministratorsIsFalse_AssignedToFilterOptionsDoNotIncludeAdministrators()
    {
        // Arrange
        SupportTaskAssignmentOptions.IncludeAdministrators = false;

        await CreateAndSetCurrentUserAsync(name: "Current User");
        var administrator = await TestData.CreateUserAsync(name: "Administrator", role: UserRoles.Administrator);
        var recordManager = await TestData.CreateUserAsync(name: "Record Manager", role: UserRoles.RecordManager);

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var optionValues = ((IHtmlSelectElement)doc.GetElementById("AssignedToUserId")!)
            .Options
            .Select(o => o.Value)
            .ToArray();

        Assert.DoesNotContain(administrator.UserId.ToString(), optionValues);
        Assert.Contains(recordManager.UserId.ToString(), optionValues);
    }

    [Fact]
    public async Task Get_FilterByUnassignedUserId_ShowsOnlyTasksThatAreNotAssigned()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(name: "User A");
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22))),
        };
        await AssignToUserAsync(tasks["ST2"], user.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/active?assignedToUserId={SupportTaskSearchService.UnassignedUserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["ST1", "ST3"], GetResultTaskKeys(doc, tasks));
    }

    [Fact]
    public async Task Get_FilterByCurrentUserId_ShowsOnlyTasksAssignedToCurrentUser()
    {
        // Arrange
        var currentUser = await CreateAndSetCurrentUserAsync();
        var otherUser = await TestData.CreateUserAsync(name: "Other User");
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20))),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21))),
        };
        await AssignToUserAsync(tasks["ST1"], currentUser.UserId);
        await AssignToUserAsync(tasks["ST2"], otherUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/active?assignedToUserId={currentUser.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["ST1"], GetResultTaskKeys(doc, tasks));
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

    [Fact]
    public async Task Get_FilterByMultipleStatuses_ShowsTasksWithAnyGivenStatus()
    {
        // Arrange
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 20)).WithStatus(SupportTaskStatus.Open)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 21)).WithStatus(SupportTaskStatus.InProgress)),
            ["ST3"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r.WithCreatedOn(new DateTime(2025, 1, 22)).WithStatus(SupportTaskStatus.Closed)),
        };

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/active?status={SupportTaskStatus.Open}&status={SupportTaskStatus.Closed}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(["ST1", "ST3"], GetResultTaskKeys(doc, tasks));
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

    [Theory]
    [InlineData(SortDirection.Ascending, new[] { "ST2", "ST1" })]
    [InlineData(SortDirection.Descending, new[] { "ST1", "ST2" })]
    public async Task Get_SortBySource_ShowsTasksInSourceApplicationShortNameOrder(SortDirection sortDirection, string[] expectedTaskKeys)
    {
        // Arrange
        var zebraApplicationUser = await TestData.CreateApplicationUserAsync(name: "Ant application", shortName: "Zebra");
        var antApplicationUser = await TestData.CreateApplicationUserAsync(name: "Zebra application", shortName: "Ant");
        var tasks = new SupportTaskLookup
        {
            ["ST1"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 20))
                .WithSourceApplicationUserId(zebraApplicationUser.UserId)),
            ["ST2"] = await TestData.CreateChangeNameRequestSupportTaskAsync(r => r
                .WithCreatedOn(new DateTime(2025, 1, 21))
                .WithSourceApplicationUserId(antApplicationUser.UserId)),
        };

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/support-tasks/active?sortBy={SupportTasksSortByOption.Source}&sortDirection={sortDirection}");

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

    [Fact]
    public async Task Get_TaskSelectedOnAnotherPage_IsCarriedOverAsHiddenInput()
    {
        // Arrange
        var selectedReference = await CreateTasksSpanningTwoPagesAndSelectOneOnFirstPageAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/active?pageNumber=2&SupportTaskReference={selectedReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal([selectedReference], GetSelectionCarriedOverFromOtherPages(doc));
        Assert.DoesNotContain(selectedReference, GetResultTaskReferences(doc));
    }

    [Fact]
    public async Task Get_TaskSelectedOnThisPage_IsCheckedAndNotCarriedOverAsHiddenInput()
    {
        // Arrange
        await TestData.CreateChangeNameRequestSupportTaskAsync();
        var selectedTask = await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/active?SupportTaskReference={selectedTask.SupportTaskReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal([selectedTask.SupportTaskReference], GetCheckedTaskReferences(doc));
        Assert.Empty(GetSelectionCarriedOverFromOtherPages(doc));
    }

    [Fact]
    public async Task Get_TasksSelectedOnThisPageAndAnother_ShowsCombinedCountInSelectionBanner()
    {
        // Arrange
        var selectedOnFirstPage = await CreateTasksSpanningTwoPagesAndSelectOneOnFirstPageAsync();

        var secondPageResponse = await HttpClient.GetAsync("/support-tasks/active?pageNumber=2");
        var selectedOnSecondPage = GetResultTaskReferences(await AssertEx.HtmlResponseAsync(secondPageResponse)).Single();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/active?pageNumber=2&SupportTaskReference={selectedOnFirstPage}&SupportTaskReference={selectedOnSecondPage}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal("2", doc.GetElementByTestId("selected-task-count")?.TrimmedText());
    }

    [Fact]
    public async Task Get_NoTasksSelected_DoesNotShowSelectionBanner()
    {
        // Arrange
        await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("selected-task-count"));
    }

    [Fact]
    public async Task Get_SelectedTaskRepeated_IsCountedOnce()
    {
        // Arrange
        var selectedReference = await CreateTasksSpanningTwoPagesAndSelectOneOnFirstPageAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/active?pageNumber=2&SupportTaskReference={selectedReference}&SupportTaskReference={selectedReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal("1", doc.GetElementByTestId("selected-task-count")?.TrimmedText());
        Assert.Equal([selectedReference], GetSelectionCarriedOverFromOtherPages(doc));
    }

    [Fact]
    public async Task Get_PaginationLinks_RequestTheNewPageWithTheSelectionIncluded()
    {
        // Arrange
        var selectedReference = await CreateTasksSpanningTwoPagesAndSelectOneOnFirstPageAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/active?SupportTaskReference={selectedReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var paginationLinks = doc.QuerySelectorAll(".govuk-pagination a").ToArray();
        Assert.NotEmpty(paginationLinks);

        foreach (var link in paginationLinks)
        {
            var href = link.GetAttribute("href");

            // The previous and next links are boosted; the numbered links request their own href
            var isBoosted = GetInheritedAttribute(link, "hx-boost") == "true";
            Assert.True(isBoosted || link.GetAttribute("hx-get") == href, $"'{link.TrimmedText()}' does not make an htmx request");

            // The URL that gets requested carries the selection, and that's what's pushed into
            // history, so going back to it restores the page with the selection intact
            Assert.Equal("true", GetInheritedAttribute(link, "hx-push-url"));
            Assert.Equal("[name=SupportTaskReference]", GetInheritedAttribute(link, "hx-include"));
            Assert.Equal("main", GetInheritedAttribute(link, "hx-target"));
            Assert.Equal("main", GetInheritedAttribute(link, "hx-select"));
            Assert.Equal("outerHTML", GetInheritedAttribute(link, "hx-swap"));
        }
    }

    [Fact]
    public async Task Get_PaginationLinks_DoNotIncludeTheSelection()
    {
        // Arrange
        var selectedReference = await CreateTasksSpanningTwoPagesAndSelectOneOnFirstPageAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/active?SupportTaskReference={selectedReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var paginationLinks = doc.QuerySelectorAll(".govuk-pagination a").ToArray();
        Assert.NotEmpty(paginationLinks);
        Assert.All(paginationLinks, link => Assert.DoesNotContain("SupportTaskReference", link.GetAttribute("href")));
    }

    [Fact]
    public async Task Get_ReturnUrlsForSelectedTasks_DoNotIncludeTheSelection()
    {
        // Arrange
        var selectedReference = await CreateTasksSpanningTwoPagesAndSelectOneOnFirstPageAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/active?sortBy=Subject&SupportTaskReference={selectedReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var returnUrl = doc.QuerySelector("#assign-tasks-form input[name='returnUrl']")?.GetAttribute("value");
        Assert.Equal("/support-tasks/active?sortBy=Subject", returnUrl);
    }

    [Fact]
    public async Task GetSelectionBanner_WithSelectedTasks_ShowsDistinctCount()
    {
        // Arrange
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/support-tasks/active/SelectionBanner?SupportTaskReference=A&SupportTaskReference=B&SupportTaskReference=A");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal("2", doc.GetElementByTestId("selected-task-count")?.TrimmedText());
    }

    [Theory]
    [InlineData(1, "1 task selected")]
    [InlineData(2, "2 tasks selected")]
    public async Task GetSelectionBanner_DescribesTheSelectedTasks(int selectedTaskCount, string expectedText)
    {
        // Arrange
        var selection = string.Join(
            "&",
            Enumerable.Range(0, selectedTaskCount).Select(i => $"SupportTaskReference=TRS-{i}"));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/support-tasks/active/SelectionBanner?{selection}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Contains(expectedText, doc.QuerySelector(".trs-tasks-selection__summary")!.TrimmedText());
    }

    [Fact]
    public async Task Get_WithSelectedTasks_ClearSelectionLinkKeepsTheFiltersAndPage()
    {
        // Arrange
        var selectedReference = await CreateTasksSpanningTwoPagesAndSelectOneOnFirstPageAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/active?sortBy=Subject&pageNumber=2&SupportTaskReference={selectedReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var clearSelectionLink = doc.GetElementByTestId("clear-selection");
        Assert.NotNull(clearSelectionLink);
        Assert.Equal("/support-tasks/active?sortBy=Subject&pageNumber=2", clearSelectionLink.GetAttribute("href"));
        Assert.Equal(clearSelectionLink.GetAttribute("href"), clearSelectionLink.GetAttribute("hx-get"));

        // The link does nothing without JavaScript, so it's hidden without it
        Assert.Contains("trs-requires-js", clearSelectionLink.ClassName!);
    }

    [Fact]
    public async Task GetSelectionBanner_ClearSelectionLinkKeepsTheFiltersAndPage()
    {
        // Arrange
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/support-tasks/active/SelectionBanner?sortBy=Subject&pageNumber=2&SupportTaskReference=A");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal("/support-tasks/active?sortBy=Subject&pageNumber=2", doc.GetElementByTestId("clear-selection")?.GetAttribute("href"));
    }

    [Fact]
    public async Task Get_SelectionBannerRequest_CarriesTheFiltersAndPage()
    {
        // Arrange
        await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active?sortBy=Subject");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var checkbox = GetResultRows(doc).Single().QuerySelector("input[type='checkbox']")!;
        Assert.Equal("/support-tasks/active/SelectionBanner?sortBy=Subject", checkbox.GetAttribute("hx-get"));
        Assert.Equal("[name=SupportTaskReference]", checkbox.GetAttribute("hx-include"));
    }

    [Fact]
    public async Task Get_SortLinks_KeepTheSelection()
    {
        // Arrange
        await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var sortButtons = doc.QuerySelectorAll("th[aria-sort] button").ToArray();
        Assert.NotEmpty(sortButtons);

        foreach (var button in sortButtons)
        {
            Assert.Equal("[name=SupportTaskReference]", button.GetAttribute("hx-include"));
            Assert.Equal("true", button.GetAttribute("hx-push-url"));
        }
    }

    [Fact]
    public async Task Get_AssignForm_CarriesTheSortSoTheAssignPageCanMatchItsOrder()
    {
        // Arrange
        await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active?sortBy=Subject&sortDirection=Descending");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal("Subject", doc.QuerySelector("#assign-tasks-form input[name='sortBy']")?.GetAttribute("value"));
        Assert.Equal("Descending", doc.QuerySelector("#assign-tasks-form input[name='sortDirection']")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_OptsOutOfTheHtmxHistoryCache()
    {
        // Arrange
        await TestData.CreateChangeNameRequestSupportTaskAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        // A cached snapshot would come back without the ticked checkboxes, since clicking one never
        // sets its checked attribute
        Assert.Equal("false", doc.QuerySelector("#assign-tasks-form")?.GetAttribute("hx-history"));
    }

    [Fact]
    public async Task GetSelectionBanner_PutsTheSelectionInTheUrl()
    {
        // Arrange
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "/support-tasks/active/SelectionBanner?sortBy=Subject&pageNumber=2&SupportTaskReference=A&SupportTaskReference=B");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(
            "/support-tasks/active?sortBy=Subject&pageNumber=2&SupportTaskReference=A&SupportTaskReference=B",
            response.Headers.GetValues("HX-Replace-Url").Single());
    }

    [Fact]
    public async Task Get_FirstPage_BackLinkLeavesTheList()
    {
        // Arrange
        await CreateTasksSpanningTwoPagesAndSelectOneOnFirstPageAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/support-tasks/active");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var backLink = doc.QuerySelector(".govuk-back-link");
        Assert.Equal("/", backLink?.GetAttribute("href"));
        Assert.Null(backLink?.GetAttribute("hx-get"));
    }

    [Fact]
    public async Task Get_LaterPage_BackLinkStepsBackAPageAndKeepsTheSelection()
    {
        // Arrange
        var selectedReference = await CreateTasksSpanningTwoPagesAndSelectOneOnFirstPageAsync();

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/active?sortBy=Subject&pageNumber=2&SupportTaskReference={selectedReference}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var backLink = doc.QuerySelector(".govuk-back-link");
        var expectedHref = "/support-tasks/active?sortBy=Subject&sortDirection=Ascending&pageNumber=1";

        Assert.Equal(expectedHref, backLink?.GetAttribute("href"));
        Assert.Equal(expectedHref, backLink?.GetAttribute("hx-get"));
        Assert.Equal("[name=SupportTaskReference]", backLink?.GetAttribute("hx-include"));
        Assert.Equal("main", backLink?.GetAttribute("hx-target"));

        // It sits outside main, so it has to be swapped out of band to stay in step with the page
        Assert.Equal("true", backLink?.GetAttribute("hx-swap-oob"));
    }

    // Creates enough tasks to fill two pages and returns the reference of a task on the first page
    private async Task<string> CreateTasksSpanningTwoPagesAndSelectOneOnFirstPageAsync()
    {
        const int pageSize = 20;

        for (var i = 0; i < pageSize + 1; i++)
        {
            await TestData.CreateChangeNameRequestSupportTaskAsync();
        }

        var response = await HttpClient.GetAsync("/support-tasks/active");
        var doc = await AssertEx.HtmlResponseAsync(response);
        return GetResultTaskReferences(doc).First();
    }

    private async Task<User> CreateAndSetCurrentUserAsync(string? name = null)
    {
        // The current user from HostFixture isn't in the database once it's been cleared,
        // so create a user we can assign tasks to and sign in as them
        var user = await TestData.CreateUserAsync(name: name, role: UserRoles.RecordManager);
        SetCurrentUser(user);
        return user;
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

    private static string[] GetSelectionCarriedOverFromOtherPages(IHtmlDocument document) =>
        document
            .QuerySelectorAll("#assign-tasks-form input[type='hidden'][name='SupportTaskReference']")
            .Select(input => input.GetAttribute("value")!)
            .ToArray();

    // htmx attributes are inherited from ancestor elements
    private static string? GetInheritedAttribute(IElement element, string name)
    {
        for (var current = element; current is not null; current = current.ParentElement)
        {
            if (current.GetAttribute(name) is string value)
            {
                return value;
            }
        }

        return null;
    }

    private static string[] GetCheckedTaskReferences(IHtmlDocument document) =>
        GetResultRows(document)
            .Select(row => row.QuerySelector("input[type='checkbox']")!)
            .Where(checkbox => checkbox.HasAttribute("checked"))
            .Select(checkbox => checkbox.GetAttribute("value")!)
            .ToArray();
}
