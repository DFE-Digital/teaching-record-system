using System.Text.RegularExpressions;
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
        await Expect(page.GetByTestId("trn")).ToHaveTextAsync(trn);
    }

    public static async Task AssertSignedInWithDormantTrnRequestAsync(this IPage page, string expectedTrnRequestId)
    {
        await page.WaitForUrlPathAsync("/test");
        await Expect(page.GetByTestId("trn-request-id")).ToHaveTextAsync(expectedTrnRequestId);
    }

    public static async Task FillDateInputAsync(this IPage page, DateOnly date)
    {
        await page.FillAsync("label:text-is('Day')", date.Day.ToString());
        await page.FillAsync("label:text-is('Month')", date.Month.ToString());
        await page.FillAsync("label:text-is('Year')", date.Year.ToString());
    }

    public static Task ClickButtonAsync(this IPage page, string text) =>
        page.ClickAsync($"button{TestBase.TextIsSelector(text)}");

    public static Task ClickGovUkButtonAsync(this IPage page, string text) =>
        page.ClickAsync($".govuk-button{TestBase.TextIsSelector(text)}");

    public static Task ClickGovUkStartButtonAsync(this IPage page) =>
        page.ClickAsync(".govuk-button--start");

    public static Task ClickBackLinkAsync(this IPage page) =>
        page.ClickAsync(".govuk-back-link");

    public static Task GoToHomePageAsync(this IPage page) =>
        page.GotoAsync("/");

    public static Task ClickLinkForElementWithTestIdAsync(this IPage page, string testId) =>
        page.GetByTestId(testId).ClickAsync();

    public static Task ClickChangeLinkForSummaryListRowWithKeyAsync(this IPage page, string key) =>
        page.Locator($".govuk-summary-list__row:has(> dt{TestBase.TextSelector(key)})").GetByText("Change").ClickAsync();

    public static Task AssertOnAddApiKeyPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/api-keys/add");

    public static Task AssertOnEditApiKeyPageAsync(this IPage page, Guid apiKeyId) =>
        page.WaitForUrlPathAsync($"/api-keys/{apiKeyId}");

    public static async Task AssertFlashMessageAsync(this IPage page, string? expectedHeader = null, string? expectedMessage = null)
    {
        if (expectedHeader != null)
        {
            await Expect(page.Locator($".govuk-notification-banner__heading{TestBase.TextIsSelector(expectedHeader)}")).ToHaveTextAsync(expectedHeader);
        }
        if (expectedMessage != null)
        {
            await Expect(page.Locator($".govuk-notification-banner p{TestBase.TextIsSelector(expectedMessage)}")).ToHaveTextAsync(expectedMessage);
        }
    }

    public static Task AssertHasErrorSummaryAsync(this IPage page) =>
        Expect(page.Locator("h2:text('There is a problem')")).ToBeVisibleAsync();

    public static async Task AssertDateInputAsync(this IPage page, DateOnly date)
    {
        await Expect(page.Locator("label:text-is('Day')")).ToHaveValueAsync(date.Day.ToString());
        await Expect(page.Locator("label:text-is('Month')")).ToHaveValueAsync(date.Month.ToString());
        await Expect(page.Locator("label:text-is('Year')")).ToHaveValueAsync(date.Year.ToString());
    }

    public static async Task AssertNameInputAsync(this IPage page, string firstName, string middleName, string lastName)
    {
        await Expect(page.Locator("text=First Name")).ToHaveValueAsync(firstName);
        await Expect(page.Locator("text=Middle Name")).ToHaveValueAsync(middleName);
        await Expect(page.Locator("text=Last Name")).ToHaveValueAsync(lastName);
    }

    public static async Task AssertDateInputEmptyAsync(this IPage page)
    {
        await Expect(page.Locator("label:text-is('Day')")).ToHaveValueAsync("");
        await Expect(page.Locator("label:text-is('Month')")).ToHaveValueAsync("");
        await Expect(page.Locator("label:text-is('Year')")).ToHaveValueAsync("");
    }

    public static async Task AssertBannerAsync(this IPage page, string title, string text)
    {
        await Expect(page.Locator("h2.govuk-notification-banner__title")).ToHaveTextAsync(title);
        await Expect(page.Locator("h3.govuk-notification-banner__heading")).ToHaveTextAsync(text);
    }

    public static async Task FillDateInputAsync(this IPage page, string id, DateOnly date)
    {
        var dateInputScope = page.Locator($"#{id}");
        await dateInputScope.GetByLabel("Day").FillAsync(date.Day.ToString());
        await dateInputScope.GetByLabel("Month").FillAsync(date.Month.ToString());
        await dateInputScope.GetByLabel("Year").FillAsync(date.Year.ToString());
    }

    public static async Task FillNameInputsAsync(this IPage page, string firstName, string middleName, string lastName)
    {
        await page.FillAsync("text=First Name", firstName);
        await page.FillAsync("text=Middle Name", middleName);
        await page.FillAsync("text=Last Name", lastName);
    }

    public static Task FillEmailInputAsync(this IPage page, string email) =>
        page.FillAsync("input[type='email']", email);

    // Fields rendered as a <select> and enhanced by accessible-autocomplete (initialised on
    // window.onload) are replaced with an <input> carrying the same id, with the now-hidden
    // <select> renamed to "{id}-select". Filling the visible input and then confirming the
    // highlighted suggestion mirrors the user interaction that commits a value to the hidden
    // select before Continue is clicked.
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

    public static Task AssertContentEqualsAsync(this IPage page, string content, string label) =>
        Expect(page.GetContentForLabel(label)).ToContainTextAsync(content);

    public static Task AssertContentContainsAsync(this IPage page, string content, string label) =>
        Expect(page.GetContentForLabel(label)).ToContainTextAsync(content);

    public static ILocator GetContentForLabel(this IPage page, string label) =>
        page.Locator($"dt{TestBase.HasTextSelector(label)}").Locator("xpath=following-sibling::dd[1]");

    public static Task AssertNoListElementAsync(this IPage page, string label) =>
        Expect(page.Locator($"dt{TestBase.HasTextSelector(label)}")).ToBeHiddenAsync();

    public static Task ClickAcceptChangeButtonAsync(this IPage page) =>
        ClickGovUkButtonAsync(page, "Accept change");

    public static Task ClickRejectChangeButtonAsync(this IPage page) =>
        ClickGovUkButtonAsync(page, "Reject change");

    public static Task ClickConfirmChangeButtonAsync(this IPage page) =>
        ClickGovUkButtonAsync(page, "Confirm change");

    public static Task ClickConfirmButtonAsync(this IPage page) =>
        ClickGovUkButtonAsync(page, "Confirm");

    public static Task ClickRejectButtonAsync(this IPage page) =>
        ClickGovUkButtonAsync(page, "Reject");

    public static Task ClickContinueButtonAsync(this IPage page) =>
        ClickGovUkButtonAsync(page, "Continue");

    public static Task ClickCancelLinkAsync(this IPage page) =>
        page.ClickAsync("a.govuk-link:contains('Cancel')");

    public static Task ClickRadioAsync(this IPage page, string value) =>
        page.Locator($"input[type='radio'][value=\"{value}\"]")
            .Locator("xpath=following-sibling::label")
            .ClickAsync();

    /// <param name="exact">
    /// Match the whole label. Pass false only for a label that is deliberately identified by a
    /// prefix; a substring match resolves to every label containing <paramref name="labelText"/>,
    /// which is ambiguous when the label is a data value that may appear inside another label.
    /// </param>
    public static Task ClickRadioByLabelAsync(this IPage page, string labelText, bool exact = true) =>
        page.GetByLabel(labelText, new() { Exact = exact }).CheckAsync();

    public static Task ClickChangeLinkAsync(this IPage page) =>
        page.GetByTestId("change-link").ClickAsync();

    public static Task FollowBannerLink(this IPage page, string message)
    {
        var link = page.GetByRole(AriaRole.Link, new() { Name = message });
        return link.ClickAsync();
    }

    public static Task AssertBannerLinksToPersonRecord(this IPage page) =>
        Expect(page.Locator("a.govuk-link")).ToHaveAttributeAsync(
            "href",
            new Regex("persons/(.+/)?[0-9a-fA-F]{8}(-[0-9a-fA-F]{4}){3}-[0-9a-fA-F]{12}/*$"));

    public static Task AssertBannerLinksToPersonRecord(this IPage page, Guid personId) =>
        Expect(page.Locator("a.govuk-link:has-text('View record (opens in a new tab)')")).ToHaveAttributeAsync(
            "href",
            new Regex(Regex.Escape($"/persons/{personId}")));

    public static Task SelectProvideAdditionalInformationsAsync(this IPage page, bool addAdditionalDetail, string? details = null, string labelText = "Enter details") =>
        page.SelectReasonMoreDetailsAsync(labelText, addAdditionalDetail, details);

    public static Task SelectReasonMoreDetailsAsync(this IPage page, bool addAdditionalDetail, string? details = null, string labelText = "Add additional detail") =>
        page.SelectReasonMoreDetailsAsync(labelText, addAdditionalDetail, details);

    public static async Task SelectReasonMoreDetailsAsync(this IPage page, string additionalDetailLabel, bool addAdditionalDetail, string? details = null)
    {
        var section = page.GetByTestId("has-additional-reason_detail-options");
        var radioButton = section.Locator($"input[type='radio'][value='{addAdditionalDetail}']");
        await radioButton.ClickAsync();

        if (details != null)
        {
            await page.FillAsync($"label{TestBase.TextIsSelector(additionalDetailLabel)}", details);
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

    public static async Task SelectProvideAdditionalInformationAsync(this IPage page, string testId, Enum provideAdditionalInformation, string? details = null)
    {
        var section = page.GetByTestId(testId);
        var option = section.Locator($".govuk-radios__item:has(input[type='radio'][value='{provideAdditionalInformation}'])");
        var radioButton = option.Locator("input");
        await radioButton.ClickAsync();

        if (details != null)
        {
            var reason = option.Locator($":scope + .govuk-radios__conditional textarea");
            await reason.FillAsync(details);
        }
    }

    public static async Task SelectUploadEvidenceAsync(this IPage page, bool uploadFile, string? evidenceFileName = null)
    {
        var radioButton = page.GetByTestId("upload-evidence-options").Locator($"input[type='radio'][value='{uploadFile}']");
        await radioButton.ClickAsync();
        if (uploadFile)
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
