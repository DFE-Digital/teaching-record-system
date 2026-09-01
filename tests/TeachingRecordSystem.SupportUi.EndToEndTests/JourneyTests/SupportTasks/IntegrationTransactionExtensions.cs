using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public static class IntegrationTransactionExtensions
{
    extension(IPage page)
    {
        public Task GoToIntegrationTransactionsPageAsync() =>
            page.GotoAsync($"/support-tasks/integration-transactions");

        public Task GoToIntegrationTransactionDetailPageAsync(long integrationTransactionId) =>
            page.GotoAsync($"/support-tasks/integration-transactions/{integrationTransactionId}/detail");

        public Task GoToIntegrationTransactionDetailRowPageAsync(long integrationTransactionId, long integrationTransactionRecordId) =>
            page.GotoAsync($"/support-tasks/integration-transactions/{integrationTransactionId}/row?integrationtransactionrecordid={integrationTransactionRecordId}");

        public Task AssertOnIntegrationTransactionDetailPageAsync(long integrationTransactionId) =>
            page.WaitForUrlPathAsync($"/support-tasks/integration-transactions/{integrationTransactionId}/detail");

        public async Task AssertOnIntegrationTransactionDetailRowPageAsync(long integrationTransactionId,
            long integrationTransactionRecordId)
        {
            await page.WaitForURLAsync(url =>
                new Uri(url).PathAndQuery.Equals(
                    $"/support-tasks/integration-transactions/{integrationTransactionId}/row?integrationtransactionrecordid={integrationTransactionRecordId}",
                    StringComparison.OrdinalIgnoreCase));
        }

        public Task GoToTeacherPensionsSupportTasks() =>
            page.GotoAsync($"/support-tasks/teacher-pensions");

        public Task AssertOnTeachersPensionsSupportTasksPageAsync() =>
            page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions");

        public Task AssertOnTeachersPensionsSupportTaskMatchesPageAsync(string taskReference) =>
            page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions/{taskReference}/resolve/matches");

        public Task AssertOnTeachersPensionsSupportTaskKeepSeparatePageAsync(string taskReference) =>
            page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions/{taskReference}/resolve/keep-record-separate");

        public Task AssertOnTeachersPensionsSupportTaskConfirmKeepSeparatePageAsync(string taskReference) =>
            page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions/{taskReference}/resolve/confirm-keep-record-separate");

        public Task AssertOnTeachersPensionsSupportTaskMergePageAsync(string taskReference) =>
            page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions/{taskReference}/resolve/merge");

        public Task AssertOnTeachersPensionsSupportTaskResolveCheckAnswersPageAsync(string taskReference) =>
            page.WaitForUrlPathAsync($"/support-tasks/teacher-pensions/{taskReference}/resolve/check-answers");
    }
}
