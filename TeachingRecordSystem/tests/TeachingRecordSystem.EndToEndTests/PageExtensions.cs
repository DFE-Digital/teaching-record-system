using Microsoft.Playwright;

namespace TeachingRecordSystem.EndToEndTests;

public static class PageExtensions
{
    public static Task WaitForUrlPathAsync(this IPage page, string path) =>
        page.WaitForURLAsync(
            url => new Uri(url).LocalPath == path,
            new PageWaitForURLOptions { WaitUntil = WaitUntilState.Commit });

    public static async Task GoToAuthorizeAccessTestStartPageAsync(this IPage page, string? trnToken = null, bool deferred = false)
    {
        var scheme = deferred ? HostFixture.DeferredFakeOneLoginAuthenticationScheme : HostFixture.FakeOneLoginAuthenticationScheme;

        var url = $"{HostFixture.AuthorizeAccessBaseUrl}/test" +
            $"?scheme={Uri.EscapeDataString(scheme)}" +
            $"&trn_token={Uri.EscapeDataString(trnToken ?? "")}";

        await page.GotoAsync(url);
    }

    public static async Task AssertSignedInAsync(this IPage page, string trn)
    {
        await page.WaitForUrlPathAsync("/test");
        Assert.Equal(trn, await page.GetByTestId("trn").InnerTextAsync());
    }

    public static async Task AssertSignedInWithDormantTrnRequestAsync(this IPage page, string expectedTrnRequestId)
    {
        await page.WaitForUrlPathAsync("/test");
        Assert.Equal(expectedTrnRequestId, await page.GetByTestId("trn-request-id").InnerTextAsync());
    }

    public static async Task FillDateInputAsync(this IPage page, DateOnly date)
    {
        await page.FillAsync("label:text-is('Day')", date.Day.ToString());
        await page.FillAsync("label:text-is('Month')", date.Month.ToString());
        await page.FillAsync("label:text-is('Year')", date.Year.ToString());
    }

    public static Task ClickButtonAsync(this IPage page, string text) =>
        page.ClickAsync($"button{TextIsSelector(text)}");

    public static Task ClickGovUkButtonAsync(this IPage page, string text) =>
        page.ClickAsync($".govuk-button{TextIsSelector(text)}");

    public static Task ClickBackLinkAsync(this IPage page) =>
        page.ClickAsync(".govuk-back-link");

    public static string TextSelector(string? text) => $":text(\"{text?.Replace("\"", "\\\"")}\")";

    public static string TextIsSelector(string? text) => $":text-is(\"{text?.Replace("\"", "\\\"")}\")";

    public static string HasTextSelector(string? text) => $":has-text(\"{text?.Replace("\"", "\\\"")}\")";
}
