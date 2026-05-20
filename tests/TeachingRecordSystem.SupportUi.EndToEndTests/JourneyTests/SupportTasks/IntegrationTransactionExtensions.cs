using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public static class IntegrationTransactionExtensions
{
    public static Task GoToIntegrationTransactionsPageAsync(this IPage page) =>
        page.GotoAsync($"/support-tasks/integration-transactions");

    public static Task GoToIntegrationTransactionDetailPageAsync(this IPage page, long integrationTransactionId) =>
        page.GotoAsync($"/support-tasks/integration-transactions/{integrationTransactionId}/detail");

    public static Task GoToIntegrationTransactionDetailRowPageAsync(this IPage page, long integrationTransactionId, long integrationTransactionRecordId) =>
        page.GotoAsync($"/support-tasks/integration-transactions/{integrationTransactionId}/row?integrationtransactionrecordid={integrationTransactionRecordId}");

    public static Task AssertOnIntegrationTransactionDetailPageAsync(this IPage page, long integrationTransactionId) =>
        page.WaitForUrlPathAsync($"/support-tasks/integration-transactions/{integrationTransactionId}/detail");

    public static async Task AssertOnIntegrationTransactionDetailRowPageAsync(
        this IPage page,
        long integrationTransactionId,
        long integrationTransactionRecordId)
    {
        await page.WaitForURLAsync(url =>
            new Uri(url).PathAndQuery.Equals(
                $"/support-tasks/integration-transactions/{integrationTransactionId}/row?integrationtransactionrecordid={integrationTransactionRecordId}",
                StringComparison.OrdinalIgnoreCase));
    }

    public static Task GoToTeacherPensionsSupportTasks(this IPage page) =>
        page.GotoAsync($"/support-tasks/teacher-pensions");

    public static Task AssertOnTeachersPensionsSupportTasksPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions");

    public static Task AssertOnTeachersPensionsSupportTaskMatchesPageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions/{taskReference}/resolve/matches");

    public static Task AssertOnTeachersPensionsSupportTaskKeepSeparatePageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions/{taskReference}/resolve/keep-record-separate");

    public static Task AssertOnTeachersPensionsSupportTaskConfirmKeepSeparatePageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions/{taskReference}/resolve/confirm-keep-record-separate");

    public static Task AssertOnTeachersPensionsSupportTaskMergePageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions/{taskReference}/resolve/merge");

    public static Task AssertOnTeachersPensionsSupportTaskResolveCheckAnswersPageAsync(this IPage page, string taskReference) =>
        page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions/{taskReference}/resolve/check-answers");
}
