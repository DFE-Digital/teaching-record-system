using Microsoft.Playwright;
using Xunit;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public static class PageExtensions
{
    public static Task WaitForUrlPathAsync(this IPage page, string path) =>
        page.WaitForURLAsync(url =>
        {
            var asUri = new Uri(url);
            return asUri.LocalPath == path;
        });

    public static async Task GoToStartPage(this IPage page)
    {
        await page.GotoAsync("/");
    }

    public static async Task AssertSignedIn(this IPage page, string trn)
    {
        await page.WaitForUrlPathAsync("/");
        Assert.Equal(trn, await page.GetByTestId("trn").InnerTextAsync());
    }

    public static Task ClickButton(this IPage page, string text) =>
        page.ClickAsync($".govuk-button:text-is('{text}')");
}
