using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public static class SupportTasksPageExtensions
{
    public static Task AssertOnChangeRequestsPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync("/support-tasks/change-requests");

    public static Task AssertOnChangeRequestDetailPageAsync(this IPage page, string caseReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/change-requests/{caseReference}");

    public static Task AssertOnAcceptChangeRequestPageAsync(this IPage page, string caseReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/change-requests/{caseReference}/accept");

    public static Task AssertOnRejectChangeRequestPageAsync(this IPage page, string caseReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/change-requests/{caseReference}/reject");

    // Ticks the checkbox for the first task shown and returns its reference
    public static async Task<string> SelectFirstTaskAsync(this IPage page)
    {
        var checkbox = page.Locator("[data-testid='results'] input[type='checkbox']").First;
        var supportTaskReference = await checkbox.GetAttributeAsync("value");

        // The checkbox itself sits underneath its label, which is stretched across the row
        await page.ClickAsync($"label[for='{await checkbox.GetAttributeAsync("id")}']");

        return supportTaskReference!;
    }

    public static Task AssertSelectedTaskCountAsync(this IPage page, int count) =>
        page.Locator($"[data-testid='selected-task-count']{TestBase.TextIsSelector(count.ToString())}").WaitForAsync();

    public static Task AssertNoTasksSelectedAsync(this IPage page) =>
        page.Locator("[data-testid='selected-task-count']").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Detached });
}
