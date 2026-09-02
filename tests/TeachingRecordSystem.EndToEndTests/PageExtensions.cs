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

    public static Task ClickGovUkStartButtonAsync(this IPage page) =>
        page.ClickAsync(".govuk-button--start");

    public static Task ClickBackLinkAsync(this IPage page) =>
        page.ClickAsync(".govuk-back-link");

    public static Task ClickChangeLinkForSummaryListRowWithKeyAsync(this IPage page, string key) =>
        page.Locator($".govuk-summary-list__row:has(> dt{TextSelector(key)})").GetByText("Change").ClickAsync();

    public static string TextSelector(string? text) => $":text(\"{text?.Replace("\"", "\\\"")}\")";

    public static string TextIsSelector(string? text) => $":text-is(\"{text?.Replace("\"", "\\\"")}\")";

    public static string HasTextSelector(string? text) => $":has-text(\"{text?.Replace("\"", "\\\"")}\")";

    public static Task<string> FindContentForLabelAsync(this IPage page, string label)
    {
        var dtElement = page.Locator($"dt{HasTextSelector(label)}");
        var ddElement = dtElement.Locator("xpath=following-sibling::dd[1]");
        return ddElement.InnerTextAsync();
    }

    public static async Task AssertContentContainsAsync(this IPage page, string content, string label)
    {
        var ddText = await page.FindContentForLabelAsync(label);
        Assert.Contains(content, ddText);
    }

    public static async Task FillAutocompleteAsync(this IPage page, string id, string name)
    {
        var input = page.Locator($"input#{id}");
        await input.FillAsync(name);

        if (name.Length == 0)
        {
            return;
        }

        await page.Locator(".autocomplete__menu--visible").WaitForAsync();
        await input.PressAsync("Enter");
    }
}
