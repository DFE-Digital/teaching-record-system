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

    public static async Task AssertFlashMessage(this IPage page, string expectedHeader)
    {
        Assert.Equal(expectedHeader, await page.InnerTextAsync($".govuk-notification-banner__heading:text-is('{expectedHeader}')"));
    }

    public static Task ClickAcceptChangeButton(this IPage page)
        => ClickButton(page, "Accept change");

    public static Task ClickRejectChangeButton(this IPage page)
        => ClickButton(page, "Reject change");

    public static Task ClickConfirmButton(this IPage page)
        => ClickButton(page, "Confirm");

    private static Task ClickButton(this IPage page, string text) =>
        page.ClickAsync($".govuk-button:text-is('{text}')");
}
