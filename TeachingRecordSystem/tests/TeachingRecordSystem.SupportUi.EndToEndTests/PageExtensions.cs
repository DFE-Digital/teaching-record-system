using Microsoft.Playwright;
using TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Alerts;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class PageExtensions
{
    public static Task WaitForUrlPathAsync(this IPage page, string path) =>
        page.WaitForURLAsync(url =>
        {
            var asUri = new Uri(url);
            return asUri.LocalPath == path;
        }, new PageWaitForURLOptions { WaitUntil = WaitUntilState.Commit });

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
            Assert.Equal(expectedHeader, await page.InnerTextAsync($".govuk-notification-banner__heading{TestBase.TextIsSelector(expectedHeader)}"));
        }
        if (expectedMessage != null)
        {
            Assert.Equal(expectedMessage, await page.InnerTextAsync($".govuk-notification-banner p{TestBase.TextIsSelector(expectedMessage)}"));
        }
    }

    public static void AssertErrorSummary(this IPage page)
    {
        var element = page.Locator("h2:text('There is a problem')");
        Assert.NotNull(element);
    }

    public static async Task AssertDateInputAsync(this IPage page, DateOnly date)
    {
        Assert.Equal(date.Day.ToString(), await page.InputValueAsync("label:text-is('Day')"));
        Assert.Equal(date.Month.ToString(), await page.InputValueAsync("label:text-is('Month')"));
        Assert.Equal(date.Year.ToString(), await page.InputValueAsync("label:text-is('Year')"));
    }

    public static async Task AssertNameInputAsync(this IPage page, string firstName, string middleName, string lastName)
    {
        Assert.Equal(firstName, await page.InputValueAsync("text=First Name"));
        Assert.Equal(middleName, await page.InputValueAsync("text=Middle Name"));
        Assert.Equal(lastName, await page.InputValueAsync("text=Last Name"));
    }

    public static async Task AssertDateInputEmptyAsync(this IPage page)
    {
        Assert.Empty(await page.InputValueAsync("label:text-is('Day')"));
        Assert.Empty(await page.InputValueAsync("label:text-is('Month')"));
        Assert.Empty(await page.InputValueAsync("label:text-is('Year')"));
    }
    public static async Task AssertBannerAsync(this IPage page, string title, string text)
    {
        var bannerTitle = page.Locator("h2.govuk-notification-banner__title");
        var bannerText = page.Locator("h3.govuk-notification-banner__heading");

        Assert.Equal(title, await bannerTitle.TextContentAsync());
        Assert.Equal(text, await bannerText.TextContentAsync());
    }

    public static async Task FillDateInputAsync(this IPage page, string id, DateOnly date)
    {
        var dateInputScope = page.Locator($"#{id}");
        await dateInputScope.GetByLabel("Day").FillAsync(date.Day.ToString());
        await dateInputScope.GetByLabel("Month").FillAsync(date.Month.ToString());
        await dateInputScope.GetByLabel("Year").FillAsync(date.Year.ToString());
        //await page.FillAsync("label:text-is('Month')", date.Month.ToString());
        //await page.FillAsync("label:text-is('Year')", date.Year.ToString());
    }

    public static async Task FillDateInputAsync(this IPage page, DateOnly date)
    {
        await page.FillAsync("label:text-is('Day')", date.Day.ToString());
        await page.FillAsync("label:text-is('Month')", date.Month.ToString());
        await page.FillAsync("label:text-is('Year')", date.Year.ToString());
    }


    public static async Task FillNameInputsAsync(this IPage page, string firstName, string middleName, string lastName)
    {
        await page.FillAsync("text=First Name", firstName);
        await page.FillAsync("text=Middle Name", middleName);
        await page.FillAsync("text=Last Name", lastName);
    }

    public static Task FillEmailInputAsync(this IPage page, string email) =>
        page.FillAsync("input[type='email']", email);

    public static async Task AssertContentEquals(this IPage page, string content, string label)
    {
        var ddText = await page.FindContentForLabel(label);
        Assert.Equal(content, ddText);
    }

    public static async Task AssertContentContains(this IPage page, string content, string label)
    {
        var ddText = await page.FindContentForLabel(label);
        Assert.Contains(content, ddText);
    }

    public static Task<string> FindContentForLabel(this IPage page, string label)
    {
        var dtElement = page.Locator($"dt{TestBase.HasTextSelector(label)}");
        var ddElement = dtElement.Locator("xpath=following-sibling::dd[1]");
        return ddElement.InnerTextAsync();
    }

    public static async Task AssertNoListElementAsync(this IPage page, string label)
    {
        var element = page.Locator($"dt{TestBase.HasTextSelector(label)}");
        Assert.False(await element.IsVisibleAsync());
    }

    public static async Task SubmitAddAlertIndexPageAsync(this IPage page, string alertType, string? details, string link, DateOnly startDate)
    {
        await page.AssertOnAddAlertTypePageAsync();
        await page.FillAsync("label:text-is('Alert type')", alertType);
        if (details != null)
        {
            await page.FillAsync("label:text-is('Details')", details);
        }

        await page.FillAsync("label:text-is('Link')", link);
        await page.FillDateInputAsync(startDate);
        await page.ClickContinueButtonAsync();
    }

    public static Task ClickAcceptChangeButtonAsync(this IPage page) =>
        ClickButtonAsync(page, "Accept change");

    public static Task ClickRejectChangeButtonAsync(this IPage page) =>
        ClickButtonAsync(page, "Reject change");

    public static Task ClickConfirmChangeButtonAsync(this IPage page) =>
        ClickButtonAsync(page, "Confirm change");

    public static Task ClickConfirmButtonAsync(this IPage page) =>
        ClickButtonAsync(page, "Confirm");

    public static Task ClickRejectButtonAsync(this IPage page) =>
        ClickButtonAsync(page, "Reject");

    public static Task ClickContinueButtonAsync(this IPage page) =>
        ClickButtonAsync(page, "Continue");

    public static Task ClickButtonAsync(this IPage page, string text) =>
        page.ClickAsync($".govuk-button{TestBase.TextIsSelector(text)}");

    public static Task ClickBackLink(this IPage page) =>
        page.ClickAsync($".govuk-back-link");

    public static Task ClickCancelLink(this IPage page) =>
        page.ClickAsync("a.govuk-link:contains('Cancel')");

    public static Task ClickRadioAsync(this IPage page, string value) =>
        page.Locator($"input[type='radio'][value=\"{value}\"]")
            .Locator("xpath=following-sibling::label")
            .ClickAsync();

    public static async Task ClickRadioByLabelAsync(this IPage page, string labelText)
    {
        var label = page.Locator($"label:has-text('{labelText}')");
        var forAttr = await label.GetAttributeAsync("for");

        var radio = page.Locator($"input[id='{forAttr}']");
        await radio.CheckAsync();
    }

    public static Task ClickChangeLink(this IPage page) =>
        page.GetByTestId("change-link").ClickAsync();

    public static Task FollowBannerLink(this IPage page, string message)
    {
        var link = page.GetByRole(AriaRole.Link, new() { Name = message });
        return link.ClickAsync();
    }

    public static async Task SelectReasonMoreDetailsAsync(this IPage page, bool addAdditionalDetail, string? details = null)
    {
        var section = page.GetByTestId("has-additional-reason_detail-options");
        var radioButton = section.Locator($"input[type='radio'][value='{addAdditionalDetail}']");
        await radioButton.ClickAsync();

        if (details != null)
        {
            await page.FillAsync("label:text-is('Add additional detail')", details);
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

    public static async Task SelectReasonFileUploadAsync(this IPage page, bool uploadFile, string? evidenceFileName = null)
    {
        var radioButton = page.GetByTestId("upload-evidence-options").Locator($"input[type='radio'][value='{uploadFile}']");
        await radioButton.ClickAsync();
        if (uploadFile == true)
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
