using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class PageExtensions
{
    extension(IPage page)
    {
        public Task WaitForUrlPathAsync(string path) =>
            page.WaitForURLAsync(url =>
            {
                var asUri = new Uri(url);
                return asUri.LocalPath == path;
            }, new PageWaitForURLOptions { WaitUntil = WaitUntilState.Commit });

        public Task GoToHomePageAsync() =>
            page.GotoAsync("/");

        public Task ClickLinkForElementWithTestIdAsync(string testId) =>
            page.GetByTestId(testId).ClickAsync();

        public Task ClickChangeLinkForSummaryListRowWithKeyAsync(string key) =>
            page.Locator($".govuk-summary-list__row:has(> dt{TestBase.TextSelector(key)})").GetByText("Change").ClickAsync();

        public Task AssertOnAddApiKeyPageAsync() =>
            page.WaitForUrlPathAsync($"/api-keys/add");

        public Task AssertOnEditApiKeyPageAsync(Guid apiKeyId) =>
            page.WaitForUrlPathAsync($"/api-keys/{apiKeyId}");

        public async Task AssertFlashMessageAsync(string? expectedHeader = null, string? expectedMessage = null)
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

        public void AssertHasErrorSummary()
        {
            var element = page.Locator("h2:text('There is a problem')");
            Assert.NotNull(element);
        }

        public async Task AssertDateInputAsync(DateOnly date)
        {
            Assert.Equal(date.Day.ToString(), await page.InputValueAsync("label:text-is('Day')"));
            Assert.Equal(date.Month.ToString(), await page.InputValueAsync("label:text-is('Month')"));
            Assert.Equal(date.Year.ToString(), await page.InputValueAsync("label:text-is('Year')"));
        }

        public async Task AssertNameInputAsync(string firstName, string middleName, string lastName)
        {
            Assert.Equal(firstName, await page.InputValueAsync("text=First Name"));
            Assert.Equal(middleName, await page.InputValueAsync("text=Middle Name"));
            Assert.Equal(lastName, await page.InputValueAsync("text=Last Name"));
        }

        public async Task AssertDateInputEmptyAsync()
        {
            Assert.Empty(await page.InputValueAsync("label:text-is('Day')"));
            Assert.Empty(await page.InputValueAsync("label:text-is('Month')"));
            Assert.Empty(await page.InputValueAsync("label:text-is('Year')"));
        }

        public async Task AssertBannerAsync(string title, string text)
        {
            var bannerTitle = (await page.Locator("h2.govuk-notification-banner__title").TextContentAsync())?.Trim();
            var bannerText = (await page.Locator("h3.govuk-notification-banner__heading").TextContentAsync())?.Trim();

            Assert.Equal(title, bannerTitle);
            Assert.Equal(text, bannerText);
        }

        public async Task FillDateInputAsync(string id, DateOnly date)
        {
            var dateInputScope = page.Locator($"#{id}");
            await dateInputScope.GetByLabel("Day").FillAsync(date.Day.ToString());
            await dateInputScope.GetByLabel("Month").FillAsync(date.Month.ToString());
            await dateInputScope.GetByLabel("Year").FillAsync(date.Year.ToString());
        }

        public async Task FillDateInputAsync(DateOnly date)
        {
            await page.FillAsync("label:text-is('Day')", date.Day.ToString());
            await page.FillAsync("label:text-is('Month')", date.Month.ToString());
            await page.FillAsync("label:text-is('Year')", date.Year.ToString());
        }

        public async Task FillNameInputsAsync(string firstName, string middleName, string lastName)
        {
            await page.FillAsync("text=First Name", firstName);
            await page.FillAsync("text=Middle Name", middleName);
            await page.FillAsync("text=Last Name", lastName);
        }

        public Task FillEmailInputAsync(string email) =>
            page.FillAsync("input[type='email']", email);

        public Task FillAutocompleteAsync(string id, string name) =>
            page.FillAsync($"input#{id}", name);

        public async Task AssertContentEqualsAsync(string content, string label)
        {
            var ddText = await page.FindContentForLabelAsync(label);
            Assert.Contains(content, ddText);
        }

        public async Task AssertContentContainsAsync(string content, string label)
        {
            var ddText = await page.FindContentForLabelAsync(label);
            Assert.Contains(content, ddText);
        }

        public Task<string> FindContentForLabelAsync(string label)
        {
            var dtElement = page.Locator($"dt{TestBase.HasTextSelector(label)}");
            var ddElement = dtElement.Locator("xpath=following-sibling::dd[1]");
            return ddElement.InnerTextAsync();
        }

        public async Task AssertNoListElementAsync(string label)
        {
            var element = page.Locator($"dt{TestBase.HasTextSelector(label)}");
            Assert.False(await element.IsVisibleAsync());
        }

        public Task ClickAcceptChangeButtonAsync() =>
            ClickButtonAsync(page, "Accept change");

        public Task ClickRejectChangeButtonAsync() =>
            ClickButtonAsync(page, "Reject change");

        public Task ClickConfirmChangeButtonAsync() =>
            ClickButtonAsync(page, "Confirm change");

        public Task ClickConfirmButtonAsync() =>
            ClickButtonAsync(page, "Confirm");

        public Task ClickRejectButtonAsync() =>
            ClickButtonAsync(page, "Reject");

        public Task ClickContinueButtonAsync() =>
            ClickButtonAsync(page, "Continue");

        public Task ClickButtonAsync(string text) =>
            page.ClickAsync($".govuk-button{TestBase.TextIsSelector(text)}");

        public Task ClickBackLinkAsync() =>
            page.ClickAsync($".govuk-back-link");

        public Task ClickCancelLinkAsync() =>
            page.ClickAsync("a.govuk-link:contains('Cancel')");

        public Task ClickRadioAsync(string value) =>
            page.Locator($"input[type='radio'][value=\"{value}\"]")
                .Locator("xpath=following-sibling::label")
                .ClickAsync();

        /// <param name="exact">
        /// Match the whole label. Pass false only for a label that is deliberately identified by a
        /// prefix; a substring match resolves to every label containing <paramref name="labelText"/>,
        /// which is ambiguous when the label is a data value that may appear inside another label.
        /// </param>
        public Task ClickRadioByLabelAsync(string labelText, bool exact = true) =>
            page.GetByLabel(labelText, new() { Exact = exact }).CheckAsync();

        public Task ClickChangeLinkAsync() =>
            page.GetByTestId("change-link").ClickAsync();

        public Task FollowBannerLink(string message)
        {
            var link = page.GetByRole(AriaRole.Link, new() { Name = message });
            return link.ClickAsync();
        }

        public async Task AssertBannerLinksToPersonRecord()
        {
            var href = await page.Locator("a.govuk-link").GetAttributeAsync("href");
            Assert.Contains("persons/", href);
            var parts = href!.Split('/', StringSplitOptions.RemoveEmptyEntries);
            Assert.True(Guid.TryParse(parts.Last(), out _));
        }

        public async Task AssertBannerLinksToPersonRecord(Guid personId)
        {
            var href = await page.Locator("a.govuk-link:has-text('View record (opens in a new tab)')").GetAttributeAsync("href");
            Assert.Contains($"/persons/{personId}", href);
        }

        public Task SelectProvideAdditionalInformationsAsync(bool addAdditionalDetail, string? details = null, string labelText = "Enter details") =>
            page.SelectReasonMoreDetailsAsync(labelText, addAdditionalDetail, details);

        public Task SelectReasonMoreDetailsAsync(bool addAdditionalDetail, string? details = null, string labelText = "Add additional detail") =>
            page.SelectReasonMoreDetailsAsync(labelText, addAdditionalDetail, details);

        public async Task SelectReasonMoreDetailsAsync(string additionalDetailLabel, bool addAdditionalDetail, string? details = null)
        {
            var section = page.GetByTestId("has-additional-reason_detail-options");
            var radioButton = section.Locator($"input[type='radio'][value='{addAdditionalDetail}']");
            await radioButton.ClickAsync();

            if (details != null)
            {
                await page.FillAsync($"label{TestBase.TextIsSelector(additionalDetailLabel)}", details);
            }
        }

        public async Task SelectChangeReasonAsync(string testId, Enum changeReason, string? details = null)
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

        public async Task SelectProvideAdditionalInformationAsync(string testId, Enum provideAdditionalInformation, string? details = null)
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

        public async Task SelectUploadEvidenceAsync(bool uploadFile, string? evidenceFileName = null)
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


    // Fields rendered as a <select> and enhanced by accessible-autocomplete (initialised on
    // window.onload) are replaced with an <input> carrying the same id, with the now-hidden
    // <select> renamed to "{id}-select". Matching on "#{id}" alone can race that enhancement and
    // latch onto the hidden <select>, which never becomes visible so the fill times out (flaky on
    // CI). Scoping the selector to the <input> tag means it only ever matches the enhanced control,
    // and Playwright's auto-wait blocks until enhancement has produced it.
}
