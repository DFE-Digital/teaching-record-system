using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public static class SupportTasksPageExtensions
{
    public static Task AssertOnSupportTasksPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync("/support-tasks");

    public static Task ClickCaseReferenceLinkChangeRequestsPageAsync(this IPage page, string caseReference) =>
        page.ClickAsync($"a{TestBase.TextIsSelector(caseReference)}");

    public static Task AssertOnChangeRequestDetailPageAsync(this IPage page, string caseReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/change-requests/{caseReference}");

    public static Task AssertOnAcceptChangeRequestPageAsync(this IPage page, string caseReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/change-requests/{caseReference}/accept");

    public static Task AssertOnRejectChangeRequestPageAsync(this IPage page, string caseReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/change-requests/{caseReference}/reject");
}
