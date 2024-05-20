using Microsoft.Playwright;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public static class PageExtensions
{
    public static Task WaitForUrlPathAsync(this IPage page, string path) =>
        page.WaitForURLAsync(url =>
        {
            var asUri = new Uri(url);
            return asUri.LocalPath == path;
        });

    public static async Task GoToTestStartPage(this IPage page, string? trnToken = null)
    {
        await page.GotoAsync(
            $"/test" +
            $"?scheme={Uri.EscapeDataString(HostFixture.FakeOneLoginAuthenticationScheme)}" +
            $"&trn_token={Uri.EscapeDataString(trnToken ?? "")}");
    }

    public static async Task AssertSignedIn(this IPage page, string trn)
    {
        await page.WaitForUrlPathAsync("/test");
        Assert.Equal(trn, await page.GetByTestId("trn").InnerTextAsync());
    }

    public static async Task FillDateInput(this IPage page, DateOnly date)
    {
        await page.FillAsync("label:text-is('Day')", date.Day.ToString());
        await page.FillAsync("label:text-is('Month')", date.Month.ToString());
        await page.FillAsync("label:text-is('Year')", date.Year.ToString());
    }

    public static Task ClickButton(this IPage page, string text) =>
        page.ClickAsync($".govuk-button:text-is('{text}')");
}
