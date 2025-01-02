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

    public static async Task GoToHomePageAsync(this IPage page)
    {
        await page.GotoAsync("/");
    }

    public static async Task GoToPersonAlertsPageAsync(this IPage page, Guid personId)
    {
        await page.GotoAsync($"/persons/{personId}/alerts");
    }

    public static async Task GoToPersonDetailPageAsync(this IPage page, Guid personId)
    {
        await page.GotoAsync($"/persons/{personId}");
    }

    public static async Task GoToPersonQualificationsPageAsync(this IPage page, Guid personId)
    {
        await page.GotoAsync($"/persons/{personId}/qualifications");
    }

    public static async Task GoToAddAlertPageAsync(this IPage page, Guid personId)
    {
        await page.GotoAsync($"/alerts/add?personId={personId}");
    }

    public static async Task GoToEditAlertDetailsPageAsync(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/details");
    }

    public static async Task GoToEditAlertStartDatePageAsync(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/start-date");
    }

    public static async Task GoToEditAlertEndDatePageAsync(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/end-date");
    }

    public static async Task GoToEditAlertLinkPageAsync(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/link");
    }

    public static async Task GoToCloseAlertPageAsync(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/close");
    }

    public static async Task GoToReopenAlertPageAsync(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/re-open");
    }

    public static async Task GoToDeleteAlertPageAsync(this IPage page, Guid alertId)
    {
        await page.GotoAsync($"/alerts/{alertId}/delete");
    }

    public static async Task GoToAddMqPageAsync(this IPage page, Guid personId)
    {
        await page.GotoAsync($"/mqs/add?personId={personId}");
    }

    public static async Task GoToUsersPageAsync(this IPage page)
    {
        await page.GotoAsync($"/users");
    }

    public static async Task GoToApplicationUsersPageAsync(this IPage page)
    {
        await page.GotoAsync($"/application-users");
    }

    public static Task ClickLinkForElementWithTestIdAsync(this IPage page, string testId) =>
        page.GetByTestId(testId).ClickAsync();

    public static Task ClickChangeLinkForSummaryListRowWithKeyAsync(this IPage page, string key) =>
        page.Locator($".govuk-summary-list__row:has(> dt:text('{key}'))").GetByText("Change").ClickAsync();

    public static async Task ClickAddAlertPersonAlertsPageAsync(this IPage page)
    {
        await page.GetByTestId($"add-alert").ClickAsync();
    }

    public static async Task ClickCloseAlertPersonAlertsPageAsync(this IPage page, Guid alertId)
    {
        await page.GetByTestId($"close-{alertId}").ClickAsync();
    }

    public static async Task ClickViewAlertPersonAlertsPageAsync(this IPage page, Guid alertId)
    {
        await page.GetByTestId($"view-alert-link-{alertId}").ClickAsync();
    }

    public static async Task ClickSupportTasksLinkInNavigationBarAsync(this IPage page)
    {
        await page.ClickAsync("a:text-is('Support tasks')");
    }

    public static async Task AssertOnSupportTasksPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync("/support-tasks");
    }

    public static async Task ClickCaseReferenceLinkChangeRequestsPageAsync(this IPage page, string caseReference)
    {
        await page.ClickAsync($"a:text-is('{caseReference}')");
    }

    public static async Task AssertOnChangeRequestDetailPageAsync(this IPage page, string caseReference)
    {
        await page.WaitForUrlPathAsync($"/change-requests/{caseReference}");
    }

    public static async Task AssertOnAcceptChangeRequestPageAsync(this IPage page, string caseReference)
    {
        await page.WaitForUrlPathAsync($"/change-requests/{caseReference}/accept");
    }

    public static async Task AssertOnRejectChangeRequestPageAsync(this IPage page, string caseReference)
    {
        await page.WaitForUrlPathAsync($"/change-requests/{caseReference}/reject");
    }

    public static async Task AssertOnPersonDetailPageAsync(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}");
    }

    public static async Task AssertOnPersonAlertsPageAsync(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/alerts");
    }

    public static async Task AssertOnPersonQualificationsPageAsync(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/qualifications");
    }

    public static async Task AssertOnAddAlertTypePageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/type");
    }

    public static async Task AssertOnAddAlertDetailsPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/details");
    }

    public static async Task AssertOnAddAlertConfirmPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/confirm");
    }

    public static async Task AssertOnAddAlertLinkPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/link");
    }

    public static async Task AssertOnAddAlertStartDatePageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/start-date");
    }

    public static async Task AssertOnAddAlertEndDatePageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/end-date");
    }

    public static async Task AssertOnAddAlertReasonPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/reason");
    }

    public static async Task AssertOnAddAlertCheckAnswersPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/check-answers");
    }

    public static async Task AssertOnEditAlertDetailsPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/details");
    }

    public static async Task AssertOnEditAlertDetailsChangeReasonPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/details/change-reason");
    }

    public static async Task AssertOnEditAlertDetailsCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/details/check-answers");
    }

    public static async Task AssertOnEditAlertStartDatePageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date");
    }

    public static async Task AssertOnEditAlertStartDateChangeReasonPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date/change-reason");
    }

    public static async Task AssertOnEditAlertStartDateCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date/check-answers");
    }

    public static async Task AssertOnEditAlertEndDatePageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date");
    }

    public static async Task AssertOnEditAlertEndDateChangeReasonPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date/change-reason");
    }

    public static async Task AssertOnEditAlertEndDateCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date/check-answers");
    }

    public static async Task AssertOnEditAlertLinkPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/link");
    }

    public static async Task AssertOnEditAlertLinkChangeReasonPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/link/change-reason");
    }

    public static async Task AssertOnEditAlertLinkCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/link/check-answers");
    }

    public static async Task AssertOnAlertDetailPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}");
    }

    public static async Task AssertOnCloseAlertPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/close");
    }

    public static async Task AssertOnCloseAlertChangeReasonPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/close/change-reason");
    }

    public static async Task AssertOnCloseAlertCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/close/check-answers");
    }

    public static async Task AssertOnReopenAlertPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/re-open");
    }

    public static async Task AssertOnReopenAlertCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/re-open/check-answers");
    }

    public static async Task AssertOnDeleteAlertPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/delete");
    }

    public static async Task AssertOnDeleteAlertCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/delete/check-answers");
    }

    public static async Task AssertOnPersonEditNamePageAsync(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-name");
    }

    public static async Task AssertOnPersonEditNameConfirmPageAsync(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-name/confirm");
    }

    public static async Task AssertOnPersonEditDateOfBirthPageAsync(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-date-of-birth");
    }

    public static async Task AssertOnPersonEditDateOfBirthConfirmPageAsync(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-date-of-birth/confirm");
    }

    public static async Task AssertOnAddMqProviderPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/mqs/add/provider");
    }

    public static async Task AssertOnAddMqSpecialismPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/mqs/add/specialism");
    }

    public static async Task AssertOnAddMqStartDatePageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/mqs/add/start-date");
    }

    public static async Task AssertOnAddMqStatusPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/mqs/add/status");
    }

    public static async Task AssertOnAddMqCheckAnswersPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/mqs/add/check-answers");
    }

    public static async Task AssertOnEditMqProviderPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider");
    }

    public static async Task AssertOnEditMqProviderReasonPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider/change-reason");
    }

    public static async Task AssertOnEditMqProviderConfirmPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider/confirm");
    }

    public static async Task AssertOnEditMqSpecialismPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism");
    }

    public static async Task AssertOnEditMqSpecialismReasonPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism/change-reason");
    }

    public static async Task AssertOnEditMqSpecialismConfirmPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism/confirm");
    }

    public static async Task AssertOnEditMqStartDatePageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date");
    }

    public static async Task AssertOnEditMqStartDateReasonPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date/change-reason");
    }

    public static async Task AssertOnEditMqStartDateConfirmPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date/confirm");
    }

    public static async Task AssertOnEditMqStatusPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status");
    }

    public static async Task AssertOnEditMqStatusReasonPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status/change-reason");
    }

    public static async Task AssertOnEditMqStatusConfirmPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status/confirm");
    }

    public static async Task AssertOnDeleteMqPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/delete");
    }

    public static async Task AssertOnDeleteMqConfirmPageAsync(this IPage page, Guid qualificationId)
    {
        await page.WaitForUrlPathAsync($"/mqs/{qualificationId}/delete/confirm");
    }

    public static async Task AssertOnUsersPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/users");
    }

    public static async Task AssertOnAddUserPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/users/add");
    }

    public static async Task AssertOnAddUserConfirmPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/users/add/confirm");
    }

    public static async Task AssertOnEditUserPageAsync(this IPage page, Guid userId)
    {
        await page.WaitForUrlPathAsync($"/users/{userId}");
    }

    public static async Task AssertOnApplicationUsersPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/application-users");
    }

    public static async Task AssertOnAddApplicationUserPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/application-users/add");
    }

    public static async Task AssertOnEditApplicationUserPageAsync(this IPage page, Guid applicationUserId)
    {
        await page.WaitForUrlPathAsync($"/application-users/{applicationUserId}");
    }

    public static async Task AssertOnAddApiKeyPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/api-keys/add");
    }

    public static async Task AssertOnEditApiKeyPageAsync(this IPage page, Guid apiKeyId)
    {
        await page.WaitForUrlPathAsync($"/api-keys/{apiKeyId}");
    }

    public static async Task AssertFlashMessageAsync(this IPage page, string expectedHeader)
    {
        Assert.Equal(expectedHeader, await page.InnerTextAsync($".govuk-notification-banner__heading:text-is('{expectedHeader}')"));
    }

    public static async Task FillDateInputAsync(this IPage page, DateOnly date)
    {
        await page.FillAsync("label:text-is('Day')", date.Day.ToString());
        await page.FillAsync("label:text-is('Month')", date.Month.ToString());
        await page.FillAsync("label:text-is('Year')", date.Year.ToString());
    }

    public static async Task FillNameInputsAsync(this IPage page, string firstName, string middleName, string lastName)
    {
        await page.FillAsync("text=First Name", firstName);
        await page.FillAsync("text=Middle Name", middleName);
        await page.FillAsync("text=Last Name", lastName);
    }

    public static async Task FillEmailInputAsync(this IPage page, string email)
    {
        await page.FillAsync("input[type='email']", email);
    }

    public static async Task SubmitAddAlertIndexPageAsync(this IPage page, string alertType, string? details, string link, DateOnly startDate)
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

    public static Task ClickAcceptChangeButtonAsync(this IPage page)
        => ClickButtonAsync(page, "Accept change");

    public static Task ClickRejectChangeButtonAsync(this IPage page)
        => ClickButtonAsync(page, "Reject change");

    public static Task ClickConfirmChangeButtonAsync(this IPage page)
        => ClickButtonAsync(page, "Confirm change");

    public static Task ClickConfirmButtonAsync(this IPage page)
        => ClickButtonAsync(page, "Confirm");

    public static Task ClickRejectButtonAsync(this IPage page)
        => ClickButtonAsync(page, "Reject");

    public static Task ClickContinueButtonAsync(this IPage page)
        => ClickButtonAsync(page, "Continue");

    public static Task ClickDeactivateButtonAsync(this IPage page)
        => ClickButtonAsync(page, "Mark alert as inactive");

    public static Task ClickReactivateButtonAsync(this IPage page)
        => ClickButtonAsync(page, "Remove inactive status");

    public static Task ClickButtonAsync(this IPage page, string text) =>
        page.ClickAsync($".govuk-button:text-is('{text}')");

    public static Task ClickBackLink(this IPage page) =>
        page.ClickAsync($".govuk-back-link");
}
