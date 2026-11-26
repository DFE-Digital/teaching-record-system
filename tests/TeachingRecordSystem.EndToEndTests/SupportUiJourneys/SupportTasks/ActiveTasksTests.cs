using System.Text.RegularExpressions;

namespace TeachingRecordSystem.EndToEndTests.SupportUiJourneys.SupportTasks;

public class ActiveTasksTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private const int TasksPerPage = 20;

    [Fact]
    public async Task SelectTasksOnMultiplePagesThenAssignThem()
    {
        // Enough tasks that there's a second page to select from
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        var firstSelectedTask = await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync(".govuk-pagination__next a");
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=2"));

        await page.AssertSelectedTaskCountAsync(1);

        var secondSelectedTask = await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(2);

        await page.ClickAsync($"button{TextIsSelector("Assign selected tasks")}");

        await page.WaitForUrlPathAsync("/support-tasks/assign");

        // Listed in the order they appeared in, not the order the form submitted them in
        await Expect(page.GetByTestId("task-reference")).ToHaveTextAsync([firstSelectedTask, secondSelectedTask]);
    }

    [Fact]
    public async Task DeselectATaskSelectedOnAnotherPage()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync(".govuk-pagination__next a");
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=2"));

        await page.ClickAsync(".govuk-pagination__prev a");
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=1"));

        // The task is still selected, so clicking it again deselects it
        await page.AssertSelectedTaskCountAsync(1);
        await page.SelectFirstTaskAsync();
        await page.AssertNoTasksSelectedAsync();
    }

    [Fact]
    public async Task GoingBackFromAssignKeepsTheSelectionMadeAcrossPages()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();

        await page.ClickAsync(".govuk-pagination__next a");
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=2"));

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(2);

        await page.ClickAsync($"button{TextIsSelector("Assign selected tasks")}");
        await page.WaitForUrlPathAsync("/support-tasks/assign");

        await page.ClickAsync(".govuk-back-link");
        await page.WaitForUrlPathAsync("/support-tasks/active");

        // Both tasks are still selected, and we're back on the page we left from
        await page.AssertSelectedTaskCountAsync(2);
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=2"));
    }

    [Fact]
    public async Task BackLinkStepsBackThroughThePagesKeepingTheSelection()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync(".govuk-pagination__next a");
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=2"));

        await page.ClickAsync(".govuk-back-link");
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=1"));

        // Back on the first page with the task still ticked, rather than out of the list entirely
        await page.AssertSelectedTaskCountAsync(1);
        await Expect(page.Locator("[data-testid='results'] tbody input[type='checkbox']:checked")).ToHaveCountAsync(1);
    }

    [Fact]
    public async Task BackLinkLeavesTheListFromTheFirstPage()
    {
        await using var createdTasks = await CreateSupportTasksAsync(1);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.ClickAsync(".govuk-back-link");

        await page.WaitForUrlPathAsync("/");
    }

    [Fact]
    public async Task BrowserBackAndForwardKeepTheSelection()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync(".govuk-pagination__next a");
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=2"));

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(2);

        await page.GoBackAsync();

        // The task picked on the first page is still ticked, not just counted
        await page.AssertSelectedTaskCountAsync(1);
        await Expect(page.Locator("[data-testid='results'] tbody input[type='checkbox']:checked")).ToHaveCountAsync(1);

        // The back link is rendered out of band, which a history restore has to be careful not to
        // strip out of the response
        await Expect(page.Locator(".govuk-back-link")).ToHaveAttributeAsync("href", "/");

        await page.GoForwardAsync();

        await page.AssertSelectedTaskCountAsync(2);
        await Expect(page.Locator("[data-testid='results'] tbody input[type='checkbox']:checked")).ToHaveCountAsync(1);
        await Expect(page.Locator(".govuk-back-link")).ToHaveAttributeAsync("href", new Regex("pageNumber=1"));
    }

    [Fact]
    public async Task SelectAllPicksUpEveryTaskOnThePageWithoutDisturbingTheOthers()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.ToggleSelectAllAsync();
        await page.AssertSelectedTaskCountAsync(TasksPerPage);
        await Expect(page.Locator("[data-testid='results'] tbody input[type=checkbox]:checked")).ToHaveCountAsync(TasksPerPage);

        await page.ClickAsync(".govuk-pagination__next a");
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=2"));

        // The first page's tasks are still selected, and the one on this page isn't
        await page.AssertSelectedTaskCountAsync(TasksPerPage);
        await Expect(page.Locator("#select-all-tasks")).Not.ToBeCheckedAsync();

        await page.ToggleSelectAllAsync();
        await page.AssertSelectedTaskCountAsync(TasksPerPage + 1);

        // Deselecting drops only the tasks on this page
        await page.ToggleSelectAllAsync();
        await page.AssertSelectedTaskCountAsync(TasksPerPage);
    }

    [Fact]
    public async Task TickingEveryRowTicksTheHeaderCheckbox()
    {
        await using var createdTasks = await CreateSupportTasksAsync(3);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        var selectAll = page.Locator("#select-all-tasks");
        var rows = await page.Locator("[data-testid='results'] tbody input[type=checkbox]").AllAsync();

        for (var i = 0; i < rows.Count; i++)
        {
            await page.ClickAsync($"label[for='{await rows[i].GetAttributeAsync("id")}']");
            await page.AssertSelectedTaskCountAsync(i + 1);

            await Expect(selectAll).ToBeCheckedAsync(new() { Checked = i == rows.Count - 1 });
        }

        // And unticking any one of them clears it again
        await page.ClickAsync($"label[for='{await rows[0].GetAttributeAsync("id")}']");
        await page.AssertSelectedTaskCountAsync(rows.Count - 1);
        await Expect(selectAll).Not.ToBeCheckedAsync();
    }

    [Fact]
    public async Task SortingKeepsTheSelection()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync($"th button{HasTextSelector("Task")}");
        await Expect(page).ToHaveURLAsync(new Regex("sortBy=Subject"));

        await page.AssertSelectedTaskCountAsync(1);
    }

    [Fact]
    public async Task ClearSelectionRemovesEverythingSelectedAcrossPages()
    {
        await using var createdTasks = await CreateSupportTasksAsync(TasksPerPage + 1);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();

        await page.ClickAsync(".govuk-pagination__next a");
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=2"));

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(2);

        await page.ClickLinkForElementWithTestIdAsync("clear-selection");

        await page.AssertNoTasksSelectedAsync();

        // Clearing the selection keeps you on the page you were on
        await Expect(page).ToHaveURLAsync(new Regex("pageNumber=2"));
    }

    [Fact]
    public async Task ChangingTheFiltersClearsTheSelection()
    {
        await using var createdTasks = await CreateSupportTasksAsync(1);

        await using var context = await HostFixture.CreateSupportUiBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync(createdTasks.ActiveTasksUrl);

        await page.SelectFirstTaskAsync();
        await page.AssertSelectedTaskCountAsync(1);

        await page.ClickAsync($"a{TextIsSelector("Clear all")}");
        await Expect(page).Not.ToHaveURLAsync(new Regex("assignedToUserId"));

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
