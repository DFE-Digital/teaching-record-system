using Microsoft.Playwright;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public static class PageExtensions
{
    public static Task WaitForUrlPathAsync(this IPage page, string path) =>
        page.WaitForURLAsync(url =>
        {
            var asUri = new Uri(url);
            return asUri.LocalPath == path;
        }, new PageWaitForURLOptions { WaitUntil = WaitUntilState.Commit });

    public static async Task GoToTestStartPageAsync(this IPage page, string? trnToken = null)
    {
        await page.GotoAsync(
            $"/test" +
            $"?scheme={Uri.EscapeDataString(HostFixture.FakeOneLoginAuthenticationScheme)}" +
            $"&trn_token={Uri.EscapeDataString(trnToken ?? "")}");
    }

    public static async Task AssertSignedInAsync(this IPage page, string trn)
    {
        await page.WaitForUrlPathAsync("/test");
        Assert.Equal(trn, await page.GetByTestId("trn").InnerTextAsync());
    }

    public static async Task FillDateInputAsync(this IPage page, DateOnly date)
    {
        await page.FillAsync("label:text-is('Day')", date.Day.ToString());
        await page.FillAsync("label:text-is('Month')", date.Month.ToString());
        await page.FillAsync("label:text-is('Year')", date.Year.ToString());
    }

    public static Task ClickButtonAsync(this IPage page, string text) =>
        page.ClickAsync($".govuk-button{TestBase.TextIsSelector(text)}");

    public static Task ClickBackLinkAsync(this IPage page) =>
        page.ClickAsync($".govuk-back-link");
}
