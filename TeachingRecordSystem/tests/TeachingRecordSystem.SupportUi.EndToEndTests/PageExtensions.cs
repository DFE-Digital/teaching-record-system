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

    public static Task ClickLinkForElementWithTestId(this IPage page, string testId) =>
        page.GetByTestId(testId).ClickAsync();

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

    public static async Task ClickOpenCasesLinkInNavigationBar(this IPage page)
    {
        await page.ClickAsync("a:text-is('Open cases')");
    }

    public static async Task AssertOnOpenCasesPage(this IPage page)
    {
        await page.WaitForUrlPathAsync("/cases");
    }

    public static async Task ClickCaseReferenceLinkOpenCasesPage(this IPage page, string caseReference)
    {
        await page.ClickAsync($"a:text-is('{caseReference}')");
    }

    public static async Task AssertOnCaseDetailPage(this IPage page, string caseReference)
    {
        await page.WaitForUrlPathAsync($"/cases/{caseReference}");
    }

    public static async Task AssertOnAcceptCasePage(this IPage page, string caseReference)
    {
        await page.WaitForUrlPathAsync($"/cases/{caseReference}/accept");
    }

    public static async Task AssertOnRejectCasePage(this IPage page, string caseReference)
    {
        await page.WaitForUrlPathAsync($"/cases/{caseReference}/reject");
    }

    public static async Task AssertOnPersonDetailPage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}");
    }

    public static async Task AssertOnPersonAlertsPage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/alerts");
    }

    public static async Task AssertOnAddAlertPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add");
    }

    public static async Task AssertOnAddAlertConfirmPage(this IPage page)
    {
        await page.WaitForUrlPathAsync($"/alerts/add/confirm");
    }

    public static async Task AssertOnAlertDetailPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}");
    }

    public static async Task AssertOnCloseAlertPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/close");
    }

    public static async Task AssertOnCloseAlertConfirmPage(this IPage page, Guid alertId)
    {
        await page.WaitForUrlPathAsync($"/alerts/{alertId}/close/confirm");
    }
    
    public static async Task AssertOnPersonEditNamePage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-name");
    }

    public static async Task AssertOnPersonEditNameConfirmPage(this IPage page, Guid personId)
    {
        await page.WaitForUrlPathAsync($"/persons/{personId}/edit-name/confirm");
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

    public static async Task SubmitAddAlertIndexPage(this IPage page, string alertType, string? details, string link, DateOnly startDate)
    {
        await page.AssertOnAddAlertPage();
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

    public static Task ClickConfirmButton(this IPage page)
        => ClickButton(page, "Confirm");

    public static Task ClickContinueButton(this IPage page)
        => ClickButton(page, "Continue");

    public static Task ClickDeactivateButton(this IPage page)
        => ClickButton(page, "Mark alert as inactive");

    public static Task ClickReactivateButton(this IPage page)
        => ClickButton(page, "Remove inactive status");

    private static Task ClickButton(this IPage page, string text) =>
        page.ClickAsync($".govuk-button:text-is('{text}')");
}
