using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.Shared;

public static class PageExtensions
{
    public static Task WaitForUrlPathAsync(this IPage page, string path) =>
        page.WaitForURLAsync(url =>
        {
            var asUri = new Uri(url);
            return asUri.LocalPath == path;
        });

    public static Task GoToHomePageAsync(this IPage page)
    {
        return page.GotoAsync("/");
    }

    public static Task GoToPersonAlertsPageAsync(this IPage page, Guid personId)
    {
        return page.GotoAsync($"/persons/{personId}/alerts");
    }

    public static Task GoToPersonDetailPageAsync(this IPage page, Guid personId)
    {
        return page.GotoAsync($"/persons/{personId}");
    }

    public static Task GoToPersonQualificationsPageAsync(this IPage page, Guid personId)
    {
        return page.GotoAsync($"/persons/{personId}/qualifications");
    }

    public static Task GoToAddAlertPageAsync(this IPage page, Guid personId)
    {
        return page.GotoAsync($"/alerts/add?personId={personId}");
    }

    public static Task GoToEditAlertDetailsPageAsync(this IPage page, Guid alertId)
    {
        return page.GotoAsync($"/alerts/{alertId}/details");
    }

    public static Task GoToEditAlertStartDatePageAsync(this IPage page, Guid alertId)
    {
        return page.GotoAsync($"/alerts/{alertId}/start-date");
    }

    public static Task GoToEditAlertEndDatePageAsync(this IPage page, Guid alertId)
    {
        return page.GotoAsync($"/alerts/{alertId}/end-date");
    }

    public static Task GoToEditAlertLinkPageAsync(this IPage page, Guid alertId)
    {
        return page.GotoAsync($"/alerts/{alertId}/link");
    }

    public static Task GoToCloseAlertPageAsync(this IPage page, Guid alertId)
    {
        return page.GotoAsync($"/alerts/{alertId}/close");
    }

    public static Task GoToReopenAlertPageAsync(this IPage page, Guid alertId)
    {
        return page.GotoAsync($"/alerts/{alertId}/re-open");
    }

    public static Task GoToDeleteAlertPageAsync(this IPage page, Guid alertId)
    {
        return page.GotoAsync($"/alerts/{alertId}/delete");
    }

    public static Task GoToAddMqPageAsync(this IPage page, Guid personId)
    {
        return page.GotoAsync($"/mqs/add?personId={personId}");
    }

    public static Task GoToUsersPageAsync(this IPage page)
    {
        return page.GotoAsync($"/users");
    }

    public static Task GoToApplicationUsersPageAsync(this IPage page)
    {
        return page.GotoAsync($"/application-users");
    }

    public static Task ClickLinkForElementWithTestIdAsync(this IPage page, string testId) =>
        page.GetByTestId(testId).ClickAsync();

    public static Task ClickChangeLinkForSummaryListRowWithKeyAsync(this IPage page, string key) =>
        page.Locator($".govuk-summary-list__row:has(> dt:text('{key}'))").GetByText("Change").ClickAsync();

    public static Task ClickAddAlertPersonAlertsPageAsync(this IPage page)
    {
        return page.GetByTestId($"add-alert").ClickAsync();
    }

    public static Task ClickCloseAlertPersonAlertsPageAsync(this IPage page, Guid alertId)
    {
        return page.GetByTestId($"close-{alertId}").ClickAsync();
    }

    public static Task ClickViewAlertPersonAlertsPageAsync(this IPage page, Guid alertId)
    {
        return page.GetByTestId($"view-alert-link-{alertId}").ClickAsync();
    }

    public static Task ClickSupportTasksLinkInNavigationBarAsync(this IPage page)
    {
        return page.ClickAsync("a:text-is('Support tasks')");
    }

    public static Task AssertOnSupportTasksPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync("/support-tasks");
    }

    public static Task ClickCaseReferenceLinkChangeRequestsPageAsync(this IPage page, string caseReference)
    {
        return page.ClickAsync($"a:text-is('{caseReference}')");
    }

    public static Task AssertOnChangeRequestDetailPageAsync(this IPage page, string caseReference)
    {
        return page.WaitForUrlPathAsync($"/change-requests/{caseReference}");
    }

    public static Task AssertOnAcceptChangeRequestPageAsync(this IPage page, string caseReference)
    {
        return page.WaitForUrlPathAsync($"/change-requests/{caseReference}/accept");
    }

    public static Task AssertOnRejectChangeRequestPageAsync(this IPage page, string caseReference)
    {
        return page.WaitForUrlPathAsync($"/change-requests/{caseReference}/reject");
    }
    public static Task AssertOnPersonDetailPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}");
    }

    public static Task AssertOnPersonAlertsPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/alerts");
    }

    public static Task AssertOnPersonQualificationsPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/qualifications");
    }

    public static Task AssertOnAddAlertTypePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/alerts/add/type");
    }

    public static Task AssertOnAddAlertDetailsPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/alerts/add/details");
    }

    public static Task AssertOnAddAlertConfirmPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/alerts/add/confirm");
    }

    public static Task AssertOnAddAlertLinkPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/alerts/add/link");
    }

    public static Task AssertOnAddAlertStartDatePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/alerts/add/start-date");
    }

    public static Task AssertOnAddAlertEndDatePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/alerts/add/end-date");
    }

    public static Task AssertOnAddAlertReasonPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/alerts/add/reason");
    }

    public static Task AssertOnAddAlertCheckAnswersPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/alerts/add/check-answers");
    }

    public static Task AssertOnEditAlertDetailsPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/details");
    }

    public static Task AssertOnEditAlertDetailsChangeReasonPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/details/change-reason");
    }

    public static Task AssertOnEditAlertDetailsCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/details/check-answers");
    }

    public static Task AssertOnEditAlertStartDatePageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date");
    }

    public static Task AssertOnEditAlertStartDateChangeReasonPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date/change-reason");
    }

    public static Task AssertOnEditAlertStartDateCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/start-date/check-answers");
    }

    public static Task AssertOnEditAlertEndDatePageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date");
    }

    public static Task AssertOnEditAlertEndDateChangeReasonPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date/change-reason");
    }

    public static Task AssertOnEditAlertEndDateCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/end-date/check-answers");
    }

    public static Task AssertOnEditAlertLinkPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/link");
    }

    public static Task AssertOnEditAlertLinkChangeReasonPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/link/change-reason");
    }

    public static Task AssertOnEditAlertLinkCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/link/check-answers");
    }

    public static Task AssertOnAlertDetailPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}");
    }

    public static Task AssertOnCloseAlertPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/close");
    }

    public static Task AssertOnCloseAlertChangeReasonPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/close/change-reason");
    }

    public static Task AssertOnCloseAlertCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/close/check-answers");
    }

    public static Task AssertOnReopenAlertPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/re-open");
    }

    public static Task AssertOnReopenAlertCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/re-open/check-answers");
    }

    public static Task AssertOnDeleteAlertPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/delete");
    }

    public static Task AssertOnDeleteAlertCheckAnswersPageAsync(this IPage page, Guid alertId)
    {
        return page.WaitForUrlPathAsync($"/alerts/{alertId}/delete/check-answers");
    }

    public static Task AssertOnPersonEditNamePageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-name");
    }

    public static Task AssertOnPersonEditNameConfirmPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-name/confirm");
    }

    public static Task AssertOnPersonEditDateOfBirthPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-date-of-birth");
    }

    public static Task AssertOnPersonEditDateOfBirthConfirmPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-date-of-birth/confirm");
    }

    public static Task AssertOnAddMqProviderPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/mqs/add/provider");
    }

    public static Task AssertOnAddMqSpecialismPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/mqs/add/specialism");
    }

    public static Task AssertOnAddMqStartDatePageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/mqs/add/start-date");
    }

    public static Task AssertOnAddMqStatusPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/mqs/add/status");
    }

    public static Task AssertOnAddMqCheckAnswersPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/mqs/add/check-answers");
    }

    public static Task AssertOnEditMqProviderPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider");
    }

    public static Task AssertOnEditMqProviderReasonPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider/change-reason");
    }

    public static Task AssertOnEditMqProviderConfirmPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider/confirm");
    }

    public static Task AssertOnEditMqSpecialismPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism");
    }

    public static Task AssertOnEditMqSpecialismReasonPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism/change-reason");
    }

    public static Task AssertOnEditMqSpecialismConfirmPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism/confirm");
    }

    public static Task AssertOnEditMqStartDatePageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date");
    }

    public static Task AssertOnEditMqStartDateReasonPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date/change-reason");
    }

    public static Task AssertOnEditMqStartDateConfirmPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date/confirm");
    }

    public static Task AssertOnEditMqStatusPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status");
    }

    public static Task AssertOnEditMqStatusReasonPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status/change-reason");
    }

    public static Task AssertOnEditMqStatusConfirmPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status/confirm");
    }

    public static Task AssertOnDeleteMqPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/delete");
    }

    public static Task AssertOnDeleteMqConfirmPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/mqs/{qualificationId}/delete/confirm");
    }

    public static Task AssertOnUsersPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/users");
    }

    public static Task AssertOnAddUserPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/users/add");
    }

    public static Task AssertOnAddUserConfirmPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/users/add/confirm");
    }

    public static Task AssertOnEditUserPageAsync(this IPage page, Guid userId)
    {
        return page.WaitForUrlPathAsync($"/users/{userId}");
    }

    public static Task AssertOnApplicationUsersPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/application-users");
    }

    public static Task AssertOnAddApplicationUserPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/application-users/add");
    }

    public static Task AssertOnEditApplicationUserPageAsync(this IPage page, Guid applicationUserId)
    {
        return page.WaitForUrlPathAsync($"/application-users/{applicationUserId}");
    }

    public static Task AssertOnAddApiKeyPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/api-keys/add");
    }

    public static Task AssertOnEditApiKeyPageAsync(this IPage page, Guid apiKeyId)
    {
        return page.WaitForUrlPathAsync($"/api-keys/{apiKeyId}");
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

    public static Task FillEmailInputAsync(this IPage page, string email)
    {
        return page.FillAsync("input[type='email']", email);
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
        => page.ClickButtonAsync("Accept change");

    public static Task ClickRejectChangeButtonAsync(this IPage page)
        => page.ClickButtonAsync("Reject change");

    public static Task ClickConfirmChangeButtonAsync(this IPage page)
        => page.ClickButtonAsync("Confirm change");

    public static Task ClickConfirmButtonAsync(this IPage page)
        => page.ClickButtonAsync("Confirm");

    public static Task ClickRejectButtonAsync(this IPage page)
        => page.ClickButtonAsync("Reject");

    public static Task ClickContinueButtonAsync(this IPage page)
        => page.ClickButtonAsync("Continue");

    public static Task ClickDeactivateButtonAsync(this IPage page)
        => page.ClickButtonAsync("Mark alert as inactive");

    public static Task ClickReactivateButtonAsync(this IPage page)
        => page.ClickButtonAsync("Remove inactive status");

    public static Task ClickButtonAsync(this IPage page, string text) =>
        page.ClickAsync($".govuk-button:text-is('{text}')");
}
