using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Alerts;

public static class AlertsPageExtensions
{
    public static Task GoToAddAlertPageAsync(this IPage page, Guid personId) =>
        page.GotoAsync($"/alerts/add?personId={personId}");

    public static Task GoToEditAlertDetailsPageAsync(this IPage page, Guid alertId) =>
        page.GotoAsync($"/alerts/{alertId}/details");

    public static Task GoToEditAlertStartDatePageAsync(this IPage page, Guid alertId) =>
        page.GotoAsync($"/alerts/{alertId}/start-date");

    public static Task GoToEditAlertEndDatePageAsync(this IPage page, Guid alertId) =>
        page.GotoAsync($"/alerts/{alertId}/end-date");

    public static Task GoToEditAlertLinkPageAsync(this IPage page, Guid alertId) =>
        page.GotoAsync($"/alerts/{alertId}/link");

    public static Task GoToCloseAlertPageAsync(this IPage page, Guid alertId) =>
        page.GotoAsync($"/alerts/{alertId}/close");

    public static Task GoToReopenAlertPageAsync(this IPage page, Guid alertId) =>
        page.GotoAsync($"/alerts/{alertId}/re-open");

    public static Task GoToDeleteAlertPageAsync(this IPage page, Guid alertId) =>
        page.GotoAsync($"/alerts/{alertId}/delete");

    public static Task ClickAddAlertPersonAlertsPageAsync(this IPage page) =>
        page.GetByTestId($"add-alert").ClickAsync();

    public static Task ClickCloseAlertPersonAlertsPageAsync(this IPage page, Guid alertId) =>
        page.GetByTestId($"close-{alertId}").ClickAsync();

    public static Task ClickViewAlertPersonAlertsPageAsync(this IPage page, Guid alertId) =>
        page.GetByTestId($"view-alert-link-{alertId}").ClickAsync();

    public static Task AssertOnAddAlertTypePageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/alerts/add/type");

    public static Task AssertOnAddAlertDetailsPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/alerts/add/details");

    public static Task AssertOnAddAlertConfirmPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/alerts/add/confirm");

    public static Task AssertOnAddAlertLinkPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/alerts/add/link");

    public static Task AssertOnAddAlertStartDatePageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/alerts/add/start-date");

    public static Task AssertOnAddAlertEndDatePageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/alerts/add/end-date");

    public static Task AssertOnAddAlertReasonPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/alerts/add/reason");

    public static Task AssertOnAddAlertCheckAnswersPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/alerts/add/check-answers");

    public static Task AssertOnEditAlertDetailsPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/details");

    public static Task AssertOnEditAlertDetailsChangeReasonPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/details/change-reason");

    public static Task AssertOnEditAlertDetailsCheckAnswersPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/details/check-answers");

    public static Task AssertOnEditAlertStartDatePageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date");

    public static Task AssertOnEditAlertStartDateChangeReasonPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date/change-reason");

    public static Task AssertOnEditAlertStartDateCheckAnswersPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date/check-answers");

    public static Task AssertOnEditAlertEndDatePageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date");

    public static Task AssertOnEditAlertEndDateChangeReasonPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date/change-reason");

    public static Task AssertOnEditAlertEndDateCheckAnswersPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date/check-answers");

    public static Task AssertOnEditAlertLinkPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/link");

    public static Task AssertOnEditAlertLinkChangeReasonPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/link/change-reason");

    public static Task AssertOnEditAlertLinkCheckAnswersPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/link/check-answers");

    public static Task AssertOnAlertDetailPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}");

    public static Task AssertOnCloseAlertPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/close");

    public static Task AssertOnCloseAlertChangeReasonPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/close/change-reason");

    public static Task AssertOnCloseAlertCheckAnswersPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/close/check-answers");

    public static Task AssertOnReopenAlertPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/re-open");

    public static Task AssertOnReopenAlertCheckAnswersPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/re-open/check-answers");

    public static Task AssertOnDeleteAlertPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/delete");

    public static Task AssertOnDeleteAlertCheckAnswersPageAsync(this IPage page, Guid alertId) =>
        page.WaitForUrlPathAsync($"/alerts/{alertId}/delete/check-answers");

    public static Task ClickDeactivateButtonAsync(this IPage page) =>
        page.ClickButtonAsync("Mark alert as inactive");

    public static Task ClickReactivateButtonAsync(this IPage page) =>
        page.ClickButtonAsync("Remove inactive status");
}
