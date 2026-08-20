namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class ActiveTasksTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private const int TasksPerPage = 20;

    [Fact]
    public async Task SelectTasksOnMultiplePagesThenAssignThem()
    {
        // Enough tasks that there's a second page to select from
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        var firstSelectedTask = await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync(".govuk-pagination__next a");
        await page.WaitForURLAsync(url => url.Contains("pageNumber=2"));

        await page.AssertSelectedTaskCountAsync(1);

        var secondSelectedTask = await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(2);

        await page.ClickAsync($"button{TextIsSelector("Assign selected tasks")}");

        await page.WaitForUrlPathAsync("/support-tasks/assign");

        // Listed in the order they appeared in, not the order the form submitted them in
        var assignedReferences = await page.GetByTestId("task-reference").AllInnerTextsAsync();
        Assert.Equal([firstSelectedTask, secondSelectedTask], assignedReferences);
    }

    [Fact]
    public async Task DeselectATaskSelectedOnAnotherPage()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync(".govuk-pagination__next a");
        await page.WaitForURLAsync(url => url.Contains("pageNumber=2"));

        await page.ClickAsync(".govuk-pagination__prev a");
        await page.WaitForURLAsync(url => url.Contains("pageNumber=1"));

        // The task is still selected, so clicking it again deselects it
        await page.AssertSelectedTaskCountAsync(1);
        await page.SelectFirstTaskAsync();
        await page.AssertNoTasksSelectedAsync();
    }

    [Fact]
    public async Task GoingBackFromAssignKeepsTheSelectionMadeAcrossPages()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();

        await page.ClickAsync(".govuk-pagination__next a");
        await page.WaitForURLAsync(url => url.Contains("pageNumber=2"));

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(2);

        await page.ClickAsync($"button{TextIsSelector("Assign selected tasks")}");
        await page.WaitForUrlPathAsync("/support-tasks/assign");

        await page.ClickAsync(".govuk-back-link");
        await page.WaitForUrlPathAsync("/support-tasks/active");

        // Both tasks are still selected, and we're back on the page we left from
        await page.AssertSelectedTaskCountAsync(2);
        Assert.Contains("pageNumber=2", page.Url);
    }

    [Fact]
    public async Task BackLinkStepsBackThroughThePagesKeepingTheSelection()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync(".govuk-pagination__next a");
        await page.WaitForURLAsync(url => url.Contains("pageNumber=2"));

        await page.ClickAsync(".govuk-back-link");
        await page.WaitForURLAsync(url => url.Contains("pageNumber=1"));

        // Back on the first page with the task still ticked, rather than out of the list entirely
        await page.AssertSelectedTaskCountAsync(1);
        Assert.Equal(1, await page.Locator("[data-testid='results'] input[type='checkbox']:checked").CountAsync());
    }

    [Fact]
    public async Task BackLinkLeavesTheListFromTheFirstPage()
    {
        await using var createdTasks = await CreateSupportTasksAsync(1);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.ClickAsync(".govuk-back-link");

        await page.WaitForUrlPathAsync("/");
    }

    [Fact]
    public async Task BrowserBackAndForwardKeepTheSelection()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync(".govuk-pagination__next a");
        await page.WaitForURLAsync(url => url.Contains("pageNumber=2"));

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(2);

        await page.GoBackAsync();

        // The task picked on the first page is still ticked, not just counted
        await page.AssertSelectedTaskCountAsync(1);
        Assert.Equal(1, await page.Locator("[data-testid='results'] input[type='checkbox']:checked").CountAsync());

        // The back link is rendered out of band, which a history restore has to be careful not to
        // strip out of the response
        Assert.Equal("/", await page.Locator(".govuk-back-link").GetAttributeAsync("href"));

        await page.GoForwardAsync();

        await page.AssertSelectedTaskCountAsync(2);
        Assert.Equal(1, await page.Locator("[data-testid='results'] input[type='checkbox']:checked").CountAsync());
        Assert.Contains("pageNumber=1", await page.Locator(".govuk-back-link").GetAttributeAsync("href"));
    }

    [Fact]
    public async Task SelectAllPicksUpEveryTaskOnThePageWithoutDisturbingTheOthers()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.ToggleSelectAllAsync();
        await page.AssertSelectedTaskCountAsync(TasksPerPage);
        Assert.Equal(TasksPerPage, await page.Locator("[data-testid='results'] input[type=checkbox]:checked").CountAsync());

        await page.ClickAsync(".govuk-pagination__next a");
        await page.WaitForURLAsync(url => url.Contains("pageNumber=2"));

        // The first page's tasks are still selected, and the one on this page isn't
        await page.AssertSelectedTaskCountAsync(TasksPerPage);
        Assert.False(await page.Locator("#select-all-tasks").IsCheckedAsync());

        await page.ToggleSelectAllAsync();
        await page.AssertSelectedTaskCountAsync(TasksPerPage + 1);

        // Deselecting drops only the tasks on this page
        await page.ToggleSelectAllAsync();
        await page.AssertSelectedTaskCountAsync(TasksPerPage);
    }

    [Fact]
    public async Task SortingKeepsTheSelection()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync($"th button{HasTextSelector("Task")}");
        await page.WaitForURLAsync(url => url.Contains("sortBy=Subject"));

        await page.AssertSelectedTaskCountAsync(1);
    }

    [Fact]
    public async Task ClearSelectionRemovesEverythingSelectedAcrossPages()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();

        await page.ClickAsync(".govuk-pagination__next a");
        await page.WaitForURLAsync(url => url.Contains("pageNumber=2"));

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(2);

        await page.ClickLinkForElementWithTestIdAsync("clear-selection");

        await page.AssertNoTasksSelectedAsync();

        // Clearing the selection keeps you on the page you were on
        Assert.Contains("pageNumber=2", page.Url);
    }

    [Fact]
    public async Task ChangingTheFiltersClearsTheSelection()
    {
        await using var createdTasks = await CreateSupportTasksAsync(1);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync($"a{TextIsSelector("Clear all")}");
        await page.WaitForURLAsync(url => !url.Contains("assignedToUserId"));

        await page.AssertNoTasksSelectedAsync();
    }

    private async Task<CreatedSupportTasks> CreateSupportTasksAsync(int count)
    {
        // The database is shared with the other end to end tests and holds whatever they left behind,
        // so assign these tasks to the signed in user - whose id is new for every run - and filter the
        // page down to them. Their CreatedOn values have to differ too: the list is ordered by it, and
        // ties leave the database free to return them in a different order each time, so a task could
        // move between pages mid-test.
        var ownerUserId = TestUsers.Administrator.UserId;
        var createdOn = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        var supportTaskReferences = new List<string>();

        for (var i = 0; i < count; i++)
        {
            var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                r => r.WithCreatedOn(createdOn.AddMinutes(i)));
            supportTaskReferences.Add(supportTask.SupportTaskReference);
        }

        await WithDbContextAsync(dbContext => dbContext.SupportTasks
            .Where(t => supportTaskReferences.Contains(t.SupportTaskReference))
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.AssignedToUserId, ownerUserId)));

        return new CreatedSupportTasks(this, supportTaskReferences, ownerUserId);
    }

    // These tests need enough tasks to fill more than one page, which is more than the other end to
    // end tests expect to find in the database they all share. Close them again once we're done so
    // they drop out of the task lists those tests are looking at.
    private sealed class CreatedSupportTasks(ActiveTasksTests test, IReadOnlyCollection<string> supportTaskReferences, Guid ownerUserId) : IAsyncDisposable
    {
        public string ActiveTasksUrl => $"/support-tasks/active?assignedToUserId={ownerUserId}";

        public async ValueTask DisposeAsync() =>
            await test.WithDbContextAsync(dbContext => dbContext.SupportTasks
                .Where(t => supportTaskReferences.Contains(t.SupportTaskReference))
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Status, SupportTaskStatus.Closed)));
    }
}
