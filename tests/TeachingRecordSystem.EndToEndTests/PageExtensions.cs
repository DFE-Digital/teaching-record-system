using Microsoft.Playwright;

namespace TeachingRecordSystem.EndToEndTests;

public static class PageExtensions
{
    extension(IPage page)
    {
        public Task WaitForUrlPathAsync(string path) =>
            page.WaitForURLAsync(
                url => new Uri(url).LocalPath == path,
                new PageWaitForURLOptions { WaitUntil = WaitUntilState.Commit });

        public async Task GoToAuthorizeAccessTestStartPageAsync(string? trnToken = null, bool deferred = false)
        {
            var scheme = deferred ? HostFixture.DeferredFakeOneLoginAuthenticationScheme : HostFixture.FakeOneLoginAuthenticationScheme;

            var url = $"{HostFixture.AuthorizeAccessBaseUrl}/test" +
                $"?scheme={Uri.EscapeDataString(scheme)}" +
                $"&trn_token={Uri.EscapeDataString(trnToken ?? "")}";

            await page.GotoAsync(url);
        }

        public async Task AssertSignedInAsync(string trn)
        {
            await page.WaitForUrlPathAsync("/test");
            Assert.Equal(trn, await page.GetByTestId("trn").InnerTextAsync());
        }

        public async Task AssertSignedInWithDormantTrnRequestAsync(string expectedTrnRequestId)
        {
            await page.WaitForUrlPathAsync("/test");
            Assert.Equal(expectedTrnRequestId, await page.GetByTestId("trn-request-id").InnerTextAsync());
        }

        public async Task FillDateInputAsync(DateOnly date)
        {
            await page.FillAsync("label:text-is('Day')", date.Day.ToString());
            await page.FillAsync("label:text-is('Month')", date.Month.ToString());
            await page.FillAsync("label:text-is('Year')", date.Year.ToString());
        }

        public Task ClickButtonAsync(string text) =>
            page.ClickAsync($"button{TextIsSelector(text)}");

        public Task ClickGovUkButtonAsync(string text) =>
            page.ClickAsync($".govuk-button{TextIsSelector(text)}");

        public Task ClickGovUkStartButtonAsync() =>
            page.ClickAsync(".govuk-button--start");

        public Task ClickBackLinkAsync() =>
            page.ClickAsync(".govuk-back-link");
    }

    public static string TextSelector(string? text) => $":text(\"{text?.Replace("\"", "\\\"")}\")";

    public static string TextIsSelector(string? text) => $":text-is(\"{text?.Replace("\"", "\\\"")}\")";

    public static string HasTextSelector(string? text) => $":has-text(\"{text?.Replace("\"", "\\\"")}\")";
}
