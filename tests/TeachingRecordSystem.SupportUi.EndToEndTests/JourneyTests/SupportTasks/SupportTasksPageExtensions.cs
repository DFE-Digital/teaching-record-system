using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public static class SupportTasksPageExtensions
{
    extension(IPage page)
    {
        public Task AssertOnChangeRequestsPageAsync() =>
            page.WaitForUrlPathAsync("/support-tasks/change-requests");

        public Task AssertOnChangeRequestDetailPageAsync(string caseReference) =>
            page.WaitForUrlPathAsync($"/support-tasks/change-requests/{caseReference}");

        public Task AssertOnAcceptChangeRequestPageAsync(string caseReference) =>
            page.WaitForUrlPathAsync($"/support-tasks/change-requests/{caseReference}/accept");

        public Task AssertOnRejectChangeRequestPageAsync(string caseReference) =>
            page.WaitForUrlPathAsync($"/support-tasks/change-requests/{caseReference}/reject");

        public async Task<string> SelectFirstTaskAsync()
        {
            var checkbox = page.Locator("[data-testid='results'] input[type='checkbox']").First;
            var supportTaskReference = await checkbox.GetAttributeAsync("value");

            // The checkbox itself sits underneath its label, which is stretched across the row
            await page.ClickAsync($"label[for='{await checkbox.GetAttributeAsync("id")}']");

            return supportTaskReference!;
        }

        public Task ToggleSelectAllAsync() =>
            page.ClickAsync("label[for='select-all-tasks']");

        public Task AssertSelectedTaskCountAsync(int count) =>
            page.Locator($"[data-testid='selected-task-count']{TestBase.TextIsSelector(count.ToString())}").WaitForAsync();

        public Task AssertNoTasksSelectedAsync() =>
            page.Locator("[data-testid='selected-task-count']").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
    }

    // Ticks the checkbox for the first task shown and returns its reference

    // The checkbox itself sits underneath its label, which is stretched across the cell
}
