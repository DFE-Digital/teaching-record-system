using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class PageExtensions
{
    public static Task WaitForUrlPathAsync(this IPage page, string path) =>
        page.WaitForURLAsync(url =>
        {
            var asUri = new Uri(url);
            return asUri.LocalPath == path;
        }, new PageWaitForURLOptions { WaitUntil = WaitUntilState.Commit });

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

    public static async Task GoToPersonCreatePageAsync(this IPage page)
    {
        await page.GotoAsync($"/persons/create");
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

    public static async Task GoToLegacyUsersPageAsync(this IPage page) =>
        await page.GotoAsync($"/legacy-users");

    public static async Task GoToUsersPageAsync(this IPage page) =>
        await page.GotoAsync($"/users");

    public static async Task GoToApplicationUsersPageAsync(this IPage page)
    {
        await page.GotoAsync($"/application-users");
    }

    public static Task ClickLinkForElementWithTestIdAsync(this IPage page, string testId) =>
        page.GetByTestId(testId).ClickAsync();

    public static Task ClickChangeLinkForSummaryListRowWithKeyAsync(this IPage page, string key) =>
        page.Locator($".govuk-summary-list__row:has(> dt{TestBase.TextSelector(key)})").GetByText("Change").ClickAsync();

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

    public static async Task AssertOnSupportTasksPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync("/support-tasks");
    }

    public static async Task ClickCaseReferenceLinkChangeRequestsPageAsync(this IPage page, string caseReference)
    {
        await page.ClickAsync($"a{TestBase.TextIsSelector(caseReference)}");
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

    public static async Task AssertOnPersonEditDetailsPageAsync(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-details");
    }

    public static Task AssertOnPersonEditDetailsNameChangeReasonPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-details/name-change-reason");
    }

    public static Task AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-details/other-details-change-reason");
    }

    public static Task AssertOnPersonEditDetailsCheckAnswersPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-details/check-answers");
    }

    public static async Task AssertOnPersonCreateIndexPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/persons/create");
    }

    public static async Task AssertOnPersonCreatePersonalDetailsPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/persons/create/personal-details");
    }

    public static Task AssertOnPersonCreateCreateReasonPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/persons/create/create-reason");
    }

    public static Task AssertOnPersonCreateCheckAnswersPageAsync(this IPage page)
    {
        return page.WaitForUrlPathAsync($"/persons/create/check-answers");
    }

    public static Task AssertOnPersonSetStatusChangeReasonPageAsync(this IPage page, Guid personId, PersonStatus targetStatus)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/set-status/{targetStatus}/change-reason");
    }

    public static Task AssertOnPersonSetStatusCheckAnswersPageAsync(this IPage page, Guid personId, PersonStatus targetStatus)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/set-status/{targetStatus}/check-answers");
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

    public static async Task AssertOnLegacyUsersPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/legacy-users");
    }

    public static async Task AssertOnAddLegacyUserPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/legacy-users/add");
    }

    public static async Task AssertOnLegacyAddUserConfirmPageAsync(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/legacy-users/add/confirm");
    }

    public static async Task AssertOnLegacyEditUserPageAsync(this IPage page, Guid userId)
    {
        await page.WaitForUrlPathAsync($"/legacy-users/{userId}");
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

    public static async Task AssertOnEditUserDeactivatePageAsync(this IPage page, Guid userId)
    {
        await page.WaitForUrlPathAsync($"/users/{userId}/deactivate");
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

    public static async Task AssertFlashMessageAsync(this IPage page, string? expectedHeader = null, string? expectedMessage = null)
    {
        if (expectedHeader != null)
        {
            Assert.Equal(expectedHeader, await page.InnerTextAsync($".govuk-notification-banner__heading{TestBase.TextIsSelector(expectedHeader)}"));
        }
        if (expectedMessage != null)
        {
            Assert.Equal(expectedMessage, await page.InnerTextAsync($".govuk-notification-banner p{TestBase.TextIsSelector(expectedMessage)}"));
        }
    }

    public static void AssertErrorSummary(this IPage page)
    {
        var element = page.Locator("h2:text('There is a problem')");
        Assert.NotNull(element);
    }

    public static async Task AssertDateInputAsync(this IPage page, DateOnly date)
    {
        Assert.Equal(date.Day.ToString(), await page.InputValueAsync("label:text-is('Day')"));
        Assert.Equal(date.Month.ToString(), await page.InputValueAsync("label:text-is('Month')"));
        Assert.Equal(date.Year.ToString(), await page.InputValueAsync("label:text-is('Year')"));
    }

    public static async Task AssertNameInputAsync(this IPage page, string firstName, string middleName, string lastName)
    {
        Assert.Equal(firstName, await page.InputValueAsync("text=First Name"));
        Assert.Equal(middleName, await page.InputValueAsync("text=Middle Name"));
        Assert.Equal(lastName, await page.InputValueAsync("text=Last Name"));
    }

    public static async Task AssertDateInputEmptyAsync(this IPage page)
    {
        Assert.Empty(await page.InputValueAsync("label:text-is('Day')"));
        Assert.Empty(await page.InputValueAsync("label:text-is('Month')"));
        Assert.Empty(await page.InputValueAsync("label:text-is('Year')"));
    }
    public static async Task AssertBannerAsync(this IPage page, string title, string text)
    {
        var bannerTitle = page.Locator("h2.govuk-notification-banner__title");
        var bannerText = page.Locator("h3.govuk-notification-banner__heading");

        Assert.Equal(title, await bannerTitle.TextContentAsync());
        Assert.Equal(text, await bannerText.TextContentAsync());
    }

    public static Task AssertOnRouteDetailPageAsync(this IPage page, Guid qualificationId)
    {
        return page.WaitForUrlPathAsync($"/route/{qualificationId}/edit/detail");
    }

    public static async Task FillDateInputAsync(this IPage page, string id, DateOnly date)
    {
        var dateInputScope = page.Locator($"#{id}");
        await dateInputScope.GetByLabel("Day").FillAsync(date.Day.ToString());
        await dateInputScope.GetByLabel("Month").FillAsync(date.Month.ToString());
        await dateInputScope.GetByLabel("Year").FillAsync(date.Year.ToString());
        //await page.FillAsync("label:text-is('Month')", date.Month.ToString());
        //await page.FillAsync("label:text-is('Year')", date.Year.ToString());
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

    public static async Task AssertContentEquals(this IPage page, string content, string label)
    {
        var ddText = await page.FindContentForLabel(label);
        Assert.Equal(content, ddText);
    }

    public static async Task AssertContentContains(this IPage page, string content, string label)
    {
        var ddText = await page.FindContentForLabel(label);
        Assert.Contains(content, ddText);
    }

    public static Task<string> FindContentForLabel(this IPage page, string label)
    {
        var dtElement = page.Locator($"dt{TestBase.HasTextSelector(label)}");
        var ddElement = dtElement.Locator("xpath=following-sibling::dd[1]");
        return ddElement.InnerTextAsync();
    }

    public static async Task AssertNoListElementAsync(this IPage page, string label)
    {
        var element = page.Locator($"dt{TestBase.HasTextSelector(label)}");
        Assert.False(await element.IsVisibleAsync());
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
        page.ClickAsync($".govuk-button{TestBase.TextIsSelector(text)}");

    public static Task ClickBackLink(this IPage page) =>
        page.ClickAsync($".govuk-back-link");

    public static Task ClickCancelLink(this IPage page) =>
        page.ClickAsync("a.govuk-link:contains('Cancel')");

    public static Task ClickRadioAsync(this IPage page, string value) =>
        page.Locator($"input[type='radio'][value=\"{value}\"]")
        .Locator("xpath=following-sibling::label")
        .ClickAsync();

    public static async Task ClickRadioByLabelAsync(this IPage page, string labelText)
    {
        var label = page.Locator($"label:has-text('{labelText}')");
        var forAttr = await label.GetAttributeAsync("for");

        var radio = page.Locator($"input[id='{forAttr}']");
        await radio.CheckAsync();
    }

    public static Task ClickChangeLink(this IPage page)
    {
        return page.GetByTestId("change-link").ClickAsync();
    }

    public static Task FollowBannerLink(this IPage page, string message)
    {
        var link = page.GetByRole(AriaRole.Link, new() { Name = message });
        return link.ClickAsync();
    }

    public static async Task SelectReasonMoreDetailsAsync(this IPage page, bool addAdditionalDetail, string? details = null)
    {
        var section = page.GetByTestId("has-additional-reason_detail-options");
        var radioButton = section.Locator($"input[type='radio'][value='{addAdditionalDetail}']");
        await radioButton.ClickAsync();

        if (details != null)
        {
            await page.FillAsync("label:text-is('Add additional detail')", details);
        }
    }

    public static async Task SelectChangeReasonAsync(this IPage page, string testId, Enum changeReason, string? details = null)
    {
        var section = page.GetByTestId(testId);
        var option = section.Locator($".govuk-radios__item:has(input[type='radio'][value='{changeReason}'])");
        var radioButton = option.Locator("input");
        await radioButton.ClickAsync();

        if (details != null)
        {
            var reason = option.Locator($":scope + .govuk-radios__conditional textarea");
            await reason.FillAsync(details);
        }
    }

    public static async Task SelectReasonFileUploadAsync(this IPage page, bool uploadFile, string? evidenceFileName = null)
    {
        var radioButton = page.GetByTestId("upload-evidence-options").Locator($"input[type='radio'][value='{uploadFile}']");
        await radioButton.ClickAsync();
        if (uploadFile == true)
        {
            if (evidenceFileName is null)
            {
                throw new ArgumentNullException(nameof(evidenceFileName), "Must set a filename to upload");
            }
            await page.GetByLabel("Upload a file")
                .SetInputFilesAsync(
                    new FilePayload()
                    {
                        Name = evidenceFileName,
                        MimeType = "image/jpeg",
                        Buffer = TestData.JpegImage
                    });
        }
    }
}
