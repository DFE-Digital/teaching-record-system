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

    public static Task ClickButton(this IPage page, string text) =>
        page.ClickAsync($".govuk-button:text-is('{text}')");
}
