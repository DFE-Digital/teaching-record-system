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
}
