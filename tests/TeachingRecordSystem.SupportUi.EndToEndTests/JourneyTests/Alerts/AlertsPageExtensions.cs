using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Alerts;

public static class AlertsPageExtensions
{
    extension(IPage page)
    {
        public Task GoToAddAlertPageAsync(Guid personId) =>
            page.GotoAsync($"/alerts/add?personId={personId}");

        public Task GoToEditAlertDetailsPageAsync(Guid alertId) =>
            page.GotoAsync($"/alerts/{alertId}/details");

        public Task GoToEditAlertStartDatePageAsync(Guid alertId) =>
            page.GotoAsync($"/alerts/{alertId}/start-date");

        public Task GoToEditAlertEndDatePageAsync(Guid alertId) =>
            page.GotoAsync($"/alerts/{alertId}/end-date");

        public Task GoToEditAlertLinkPageAsync(Guid alertId) =>
            page.GotoAsync($"/alerts/{alertId}/link");

        public Task GoToCloseAlertPageAsync(Guid alertId) =>
            page.GotoAsync($"/alerts/{alertId}/close");

        public Task GoToReopenAlertPageAsync(Guid alertId) =>
            page.GotoAsync($"/alerts/{alertId}/reopen");

        public Task GoToDeleteAlertPageAsync(Guid alertId) =>
            page.GotoAsync($"/alerts/{alertId}/delete");

        public Task ClickAddAlertPersonAlertsPageAsync() =>
            page.GetByTestId($"add-alert").ClickAsync();

        public Task ClickCloseAlertPersonAlertsPageAsync(Guid alertId) =>
            page.GetByTestId($"close-{alertId}").ClickAsync();

        public Task ClickViewAlertPersonAlertsPageAsync(Guid alertId) =>
            page.GetByTestId($"view-alert-link-{alertId}").ClickAsync();

        public Task AssertOnAddAlertTypePageAsync() =>
            page.WaitForUrlPathAsync($"/alerts/add/type");

        public Task AssertOnAddAlertDetailsPageAsync() =>
            page.WaitForUrlPathAsync($"/alerts/add/details");

        public Task AssertOnAddAlertConfirmPageAsync() =>
            page.WaitForUrlPathAsync($"/alerts/add/confirm");

        public Task AssertOnAddAlertLinkPageAsync() =>
            page.WaitForUrlPathAsync($"/alerts/add/link");

        public Task AssertOnAddAlertStartDatePageAsync() =>
            page.WaitForUrlPathAsync($"/alerts/add/start-date");

        public Task AssertOnAddAlertEndDatePageAsync() =>
            page.WaitForUrlPathAsync($"/alerts/add/end-date");

        public Task AssertOnAddAlertReasonPageAsync() =>
            page.WaitForUrlPathAsync($"/alerts/add/reason");

        public Task AssertOnAddAlertCheckAnswersPageAsync() =>
            page.WaitForUrlPathAsync($"/alerts/add/check-answers");

        public Task AssertOnEditAlertDetailsPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/details");

        public Task AssertOnEditAlertDetailsChangeReasonPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/details/reason");

        public Task AssertOnEditAlertDetailsCheckAnswersPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/details/check-answers");

        public Task AssertOnEditAlertStartDatePageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date");

        public Task AssertOnEditAlertStartDateChangeReasonPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date/reason");

        public Task AssertOnEditAlertStartDateCheckAnswersPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date/check-answers");

        public Task AssertOnEditAlertEndDatePageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date");

        public Task AssertOnEditAlertEndDateChangeReasonPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date/reason");

        public Task AssertOnEditAlertEndDateCheckAnswersPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date/check-answers");

        public Task AssertOnEditAlertLinkPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/link");

        public Task AssertOnEditAlertLinkChangeReasonPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/link/reason");

        public Task AssertOnEditAlertLinkCheckAnswersPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/link/check-answers");

        public Task AssertOnAlertDetailPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}");

        public Task AssertOnCloseAlertPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/close");

        public Task AssertOnCloseAlertChangeReasonPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/close/reason");

        public Task AssertOnCloseAlertCheckAnswersPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/close/check-answers");

        public Task AssertOnReopenAlertPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/reopen");

        public Task AssertOnReopenAlertCheckAnswersPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/reopen/check-answers");

        public Task AssertOnDeleteAlertPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/delete");

        public Task AssertOnDeleteAlertCheckAnswersPageAsync(Guid alertId) =>
            page.WaitForUrlPathAsync($"/alerts/{alertId}/delete/check-answers");

        public Task ClickDeactivateButtonAsync() =>
            page.ClickButtonAsync("Mark alert as inactive");

        public Task ClickReactivateButtonAsync() =>
            page.ClickButtonAsync("Remove inactive status");

        public async Task SubmitAddAlertIndexPageAsync(string alertType, string? details, string link, DateOnly startDate)
        {
            await page.AssertOnAddAlertTypePageAsync();
            await page.FillAsync("label:text-is('Alert type')", alertType);
            if (details != null)
            {
                await page.FillAsync("label:text-is('Details')", details);
            }

            await page.FillAsync("label:text-is('Link')", link);
            await page.FillDateInputAsync(startDate);
            await page.ClickContinueButtonAsync();
        }
    }
}
