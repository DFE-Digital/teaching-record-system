using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class PageExtensions
{
    public static Task WaitForUrlPathAsync(this IPage page, string path) =>
        page.WaitForURLAsync(url =>
        {
            var asUri = new Uri(url);
            return asUri.LocalPath == path;
        });

    public static async Task GoToHomePage(this IPage page)
    {
        await page.GotoAsync("/");
    }

    public static async Task GoToPersonAlertsPage(this IPage page, Guid personId)
    {
        await page.GotoAsync($"/persons/{personId}/alerts");
    }

    public static async Task GoToPersonDetailPage(this IPage page, Guid personId)
    {
        await page.GotoAsync($"/persons/{personId}");
    }

    public static async Task GoToPersonQualificationsPage(this IPage page, Guid personId)
    {
        await page.GotoAsync($"/persons/{personId}/qualifications");
    }

    public static async Task GoToAddAlertPage(this IPage page, Guid personId)
    {
        await page.GotoAsync($"/alerts/add?personId={personId}");
    }

    public static async Task GoToEditAlertDetailsPage(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/details");
    }

    public static async Task GoToEditAlertStartDatePage(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/start-date");
    }

    public static async Task GoToEditAlertEndDatePage(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/end-date");
    }

    public static async Task GoToEditAlertLinkPage(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/link");
    }

    public static async Task GoToCloseAlertPage(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/close");
    }

    public static async Task GoToReopenAlertPage(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/re-open");
    }

    public static async Task GoToDeleteAlertPage(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/delete");
    }

    public static async Task GoToAddMqPage(this IPage page, Guid personId)
    {
        await page.GotoAsync($"/mqs/add?personId={personId}");
    }

    public static async Task GoToUsersPage(this IPage page)
    {
        await page.GotoAsync($"/users");
    }

    public static async Task GoToApplicationUsersPage(this IPage page)
    {
        await page.GotoAsync($"/application-users");
    }

    public static Task ClickLinkForElementWithTestId(this IPage page, string testId) =>
        page.GetByTestId(testId).ClickAsync();

    public static Task ClickChangeLinkForSummaryListRowWithKey(this IPage page, string key) =>
        page.Locator($".govuk-summary-list__row:has(> dt:text('{key}'))").GetByText("Change").ClickAsync();

    public static async Task ClickAddAlertPersonAlertsPage(this IPage page)
    {
        await page.GetByTestId($"add-alert").ClickAsync();
    }

    public static async Task ClickCloseAlertPersonAlertsPage(this IPage page, Guid alertId)
    {
        await page.GetByTestId($"close-{alertId}").ClickAsync();
    }

    public static async Task ClickViewAlertPersonAlertsPage(this IPage page, Guid alertId)
    {
        await page.GetByTestId($"view-alert-link-{alertId}").ClickAsync();
    }

    public static async Task ClickSupportTasksLinkInNavigationBar(this IPage page)
    {
        await page.ClickAsync("a:text-is('Support tasks')");
    }

    public static async Task AssertOnSupportTasksPage(this IPage page)
    {
        await page.WaitForUrlPathAsync("/support-tasks");
    }

    public static async Task ClickCaseReferenceLinkChangeRequestsPage(this IPage page, string caseReference)
    {
        await page.ClickAsync($"a:text-is('{caseReference}')");
    }

    public static async Task AssertOnChangeRequestDetailPage(this IPage page, string caseReference)
    {
        await page.WaitForUrlPathAsync($"/change-requests/{caseReference}");
    }

    public static async Task AssertOnAcceptChangeRequestPage(this IPage page, string caseReference)
    {
        await page.WaitForUrlPathAsync($"/change-requests/{caseReference}/accept");
    }

    public static async Task AssertOnRejectChangeRequestPage(this IPage page, string caseReference)
    {
        await page.WaitForUrlPathAsync($"/change-requests/{caseReference}/reject");
    }

    public static async Task AssertOnPersonDetailPage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}");
    }

    public static async Task AssertOnPersonAlertsPage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/alerts");
    }

    public static async Task AssertOnPersonQualificationsPage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/qualifications");
    }

    public static async Task AssertOnAddAlertTypePage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/type");
    }

    public static async Task AssertOnAddAlertDetailsPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/details");
    }

    public static async Task AssertOnAddAlertConfirmPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/confirm");
    }

    public static async Task AssertOnAddAlertLinkPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/link");
    }

    public static async Task AssertOnAddAlertStartDatePage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/start-date");
    }

    public static async Task AssertOnAddAlertEndDatePage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/end-date");
    }

    public static async Task AssertOnAddAlertReasonPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/reason");
    }

    public static async Task AssertOnAddAlertCheckAnswersPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/check-answers");
    }

    public static async Task AssertOnEditAlertDetailsPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/details");
    }

    public static async Task AssertOnEditAlertDetailsChangeReasonPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/details/change-reason");
    }

    public static async Task AssertOnEditAlertDetailsCheckAnswersPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/details/check-answers");
    }

    public static async Task AssertOnEditAlertStartDatePage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date");
    }

    public static async Task AssertOnEditAlertStartDateChangeReasonPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date/change-reason");
    }

    public static async Task AssertOnEditAlertStartDateCheckAnswersPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date/check-answers");
    }

    public static async Task AssertOnEditAlertEndDatePage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date");
    }

    public static async Task AssertOnEditAlertEndDateChangeReasonPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date/change-reason");
    }

    public static async Task AssertOnEditAlertEndDateCheckAnswersPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date/check-answers");
    }

    public static async Task AssertOnEditAlertLinkPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/link");
    }

    public static async Task AssertOnEditAlertLinkChangeReasonPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/link/change-reason");
    }

    public static async Task AssertOnEditAlertLinkCheckAnswersPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/link/check-answers");
    }

    public static async Task AssertOnAlertDetailPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}");
    }

    public static async Task AssertOnCloseAlertPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/close");
    }

    public static async Task AssertOnCloseAlertChangeReasonPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/close/change-reason");
    }

    public static async Task AssertOnCloseAlertCheckAnswersPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/close/check-answers");
    }

    public static async Task AssertOnReopenAlertPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/re-open");
    }

    public static async Task AssertOnReopenAlertCheckAnswersPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/re-open/check-answers");
    }

    public static async Task AssertOnDeleteAlertPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/delete");
    }

    public static async Task AssertOnDeleteAlertConfirmPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/delete/confirm");
    }

    public static async Task AssertOnPersonEditNamePage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-name");
    }

    public static async Task AssertOnPersonEditNameConfirmPage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-name/confirm");
    }

    public static async Task AssertOnPersonEditDateOfBirthPage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-date-of-birth");
    }

    public static async Task AssertOnPersonEditDateOfBirthConfirmPage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-date-of-birth/confirm");
    }

    public static async Task AssertOnAddMqProviderPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/mqs/add/provider");
    }

    public static async Task AssertOnAddMqSpecialismPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/mqs/add/specialism");
    }

    public static async Task AssertOnAddMqStartDatePage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/mqs/add/start-date");
    }

    public static async Task AssertOnAddMqStatusPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/mqs/add/status");
    }

    public static async Task AssertOnAddMqCheckAnswersPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/mqs/add/check-answers");
    }

    public static async Task AssertOnEditMqProviderPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider");
    }

    public static async Task AssertOnEditMqProviderReasonPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider/change-reason");
    }

    public static async Task AssertOnEditMqProviderConfirmPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider/confirm");
    }

    public static async Task AssertOnEditMqSpecialismPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism");
    }

    public static async Task AssertOnEditMqSpecialismReasonPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism/change-reason");
    }

    public static async Task AssertOnEditMqSpecialismConfirmPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism/confirm");
    }

    public static async Task AssertOnEditMqStartDatePage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date");
    }

    public static async Task AssertOnEditMqStartDateReasonPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date/change-reason");
    }

    public static async Task AssertOnEditMqStartDateConfirmPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date/confirm");
    }

    public static async Task AssertOnEditMqStatusPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status");
    }

    public static async Task AssertOnEditMqStatusReasonPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status/change-reason");
    }

    public static async Task AssertOnEditMqStatusConfirmPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status/confirm");
    }

    public static async Task AssertOnDeleteMqPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/delete");
    }

    public static async Task AssertOnDeleteMqConfirmPage(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/delete/confirm");
    }

    public static async Task AssertOnUsersPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/users");
    }

    public static async Task AssertOnAddUserPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/users/add");
    }

    public static async Task AssertOnAddUserConfirmPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/users/add/confirm");
    }

    public static async Task AssertOnEditUserPage(this IPage page, Guid userId)
    {
        await page.WaitForUrlPathAsync($"/users/{userId}");
    }

    public static async Task AssertOnApplicationUsersPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/application-users");
    }

    public static async Task AssertOnAddApplicationUserPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/application-users/add");
    }

    public static async Task AssertOnEditApplicationUserPage(this IPage page, Guid applicationUserId)
    {
        await page.WaitForUrlPathAsync($"/application-users/{applicationUserId}");
    }

    public static async Task AssertOnAddApiKeyPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/api-keys/add");
    }

    public static async Task AssertOnEditApiKeyPage(this IPage page, Guid apiKeyId)
    {
        await page.WaitForUrlPathAsync($"/api-keys/{apiKeyId}");
    }

    public static async Task AssertFlashMessage(this IPage page, string expectedHeader)
    {
        Assert.Equal(expectedHeader, await page.InnerTextAsync($".govuk-notification-banner__heading:text-is('{expectedHeader}')"));
    }

    public static async Task FillDateInput(this IPage page, DateOnly date)
    {
        await page.FillAsync("label:text-is('Day')", date.Day.ToString());
        await page.FillAsync("label:text-is('Month')", date.Month.ToString());
        await page.FillAsync("label:text-is('Year')", date.Year.ToString());
    }

    public static async Task FillNameInputs(this IPage page, string firstName, string middleName, string lastName)
    {
        await page.FillAsync("text=First Name", firstName);
        await page.FillAsync("text=Middle Name", middleName);
        await page.FillAsync("text=Last Name", lastName);
    }

    public static async Task FillEmailInput(this IPage page, string email)
    {
        await page.FillAsync("input[type='email']", email);
    }

    public static async Task SubmitAddAlertIndexPage(this IPage page, string alertType, string? details, string link, DateOnly startDate)
    {
        await page.AssertOnAddAlertTypePage();
        await page.FillAsync("label:text-is('Alert type')", alertType);
        if (details != null)
        {
            await page.FillAsync("label:text-is('Details')", details);
        }

        await page.FillAsync("label:text-is('Link')", link);
        await page.FillDateInput(startDate);
        await page.ClickContinueButton();
    }

    public static Task ClickAcceptChangeButton(this IPage page)
        => ClickButton(page, "Accept change");

    public static Task ClickRejectChangeButton(this IPage page)
        => ClickButton(page, "Reject change");

    public static Task ClickConfirmChangeButton(this IPage page)
        => ClickButton(page, "Confirm change");

    public static Task ClickConfirmButton(this IPage page)
        => ClickButton(page, "Confirm");

    public static Task ClickRejectButton(this IPage page)
        => ClickButton(page, "Reject");

    public static Task ClickContinueButton(this IPage page)
        => ClickButton(page, "Continue");

    public static Task ClickDeactivateButton(this IPage page)
        => ClickButton(page, "Mark alert as inactive");

    public static Task ClickReactivateButton(this IPage page)
        => ClickButton(page, "Remove inactive status");

    public static Task ClickButton(this IPage page, string text) =>
        page.ClickAsync($".govuk-button:text-is('{text}')");
}
