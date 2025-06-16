using Microsoft.Playwright;
using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;
using TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.EndDate;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;
using TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.StartDate;
using TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class AlertTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task AddAlert()
    {
        var person = await TestData.CreatePersonAsync();
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeByIdAsync(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
        var details = TestData.GenerateLoremIpsum();
        var link = TestData.GenerateUrl();
        var startDate = new DateOnly(2021, 1, 1);
        var reason = AddAlertReasonOption.AnotherReason;
        var reasonDetail = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonAlertsPageAsync(personId);

        await page.ClickButtonAsync("Add an alert");

        await page.AssertOnAddAlertTypePageAsync();

        await page.CheckAsync($"label{TextIsSelector(alertType.Name)}");

        await page.ClickContinueButtonAsync();

        await page.AssertOnAddAlertDetailsPageAsync();

        await page.FillAsync($"label{TextIsSelector($"Enter details about the alert type: {alertType.Name}")}", details);

        await page.ClickContinueButtonAsync();

        await page.AssertOnAddAlertLinkPageAsync();

        await page.CheckAsync("label:text-is('Yes')");

        await page.FillAsync("label:text-is('Enter link to panel outcome')", link);

        await page.ClickContinueButtonAsync();

        await page.AssertOnAddAlertStartDatePageAsync();

        await page.FillDateInputAsync(startDate);

        await page.ClickContinueButtonAsync();

        await page.AssertOnAddAlertReasonPageAsync();

        await page.Locator("div.govuk-form-group:has-text('Select a reason')").Locator($"label{TextIsSelector(reason.GetDisplayName())}").CheckAsync();
        await page.Locator("div.govuk-form-group:has-text('Do you want to add more information about why you’re adding this alert?')").Locator("label:text-is('Yes')").CheckAsync();
        await page.FillAsync("label:text-is('Add additional detail')", reasonDetail);
        await page.Locator("div.govuk-form-group:has-text('Do you want to upload evidence?')").Locator("label:text-is('Yes')").CheckAsync();
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButtonAsync();

        await page.AssertOnAddAlertCheckAnswersPageAsync();

        await page.ClickButtonAsync("Add alert");

        await page.AssertOnPersonAlertsPageAsync(personId);

        await page.AssertFlashMessageAsync("Alert added");
    }

    [Fact]
    public async Task EditAlertDetails()
    {
        var startDate = new DateOnly(2023, 1, 1);
        var details = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePersonAsync(b => b.WithAlert(a => a.WithStartDate(startDate).WithDetails(details)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var newDetails = TestData.GenerateLoremIpsum();
        var reason = AlertChangeDetailsReasonOption.ChangeOfDetails;
        var reasonDetail = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToEditAlertDetailsPageAsync(alertId);

        await page.AssertOnEditAlertDetailsPageAsync(alertId);

        await page.FillAsync("label:text-is('Change details')", newDetails);

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditAlertDetailsChangeReasonPageAsync(alertId);

        await page.Locator("div.govuk-form-group:has-text('Select a reason')").Locator($"label{TextIsSelector(reason.GetDisplayName())}").CheckAsync();
        await page.Locator("div.govuk-form-group:has-text('Do you want to add more information about why you’re changing the alert details?')").Locator("label:text-is('Yes')").CheckAsync();
        await page.FillAsync("label:text-is('Add additional detail')", reasonDetail);
        await page.Locator("div.govuk-form-group:has-text('Do you want to upload evidence?')").Locator("label:text-is('Yes')").CheckAsync();
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditAlertDetailsCheckAnswersPageAsync(alertId);

        await page.ClickConfirmChangeButtonAsync();

        await page.AssertOnPersonAlertsPageAsync(personId);

        await page.AssertFlashMessageAsync("Alert changed");
    }

    [Fact]
    public async Task EditAlertStartDate()
    {
        var startDate = new DateOnly(2023, 1, 1);
        var person = await TestData.CreatePersonAsync(b => b.WithAlert(a => a.WithStartDate(startDate)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var newStartDate = new DateOnly(2023, 2, 3);
        var reason = AlertChangeStartDateReasonOption.AnotherReason;
        var reasonDetail = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToEditAlertStartDatePageAsync(alertId);

        await page.AssertOnEditAlertStartDatePageAsync(alertId);

        await page.FillDateInputAsync(newStartDate);

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditAlertStartDateChangeReasonPageAsync(alertId);

        await page.Locator("div.govuk-form-group:has-text('Select a reason')").Locator($"label{TextIsSelector(reason.GetDisplayName())}").CheckAsync();
        await page.Locator("div.govuk-form-group:has-text('Do you want to add more information about why you’re changing the start date?')").Locator("label:text-is('Yes')").CheckAsync();
        await page.FillAsync("label:text-is('Add additional detail')", reasonDetail);
        await page.Locator("div.govuk-form-group:has-text('Do you want to upload evidence?')").Locator("label:text-is('Yes')").CheckAsync();
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditAlertStartDateCheckAnswersPageAsync(alertId);

        await page.ClickConfirmChangeButtonAsync();

        await page.AssertOnPersonAlertsPageAsync(personId);

        await page.AssertFlashMessageAsync("Alert changed");
    }

    [Fact]
    public async Task EditAlertEndDate()
    {
        var startDate = TestData.Clock.Today.AddDays(-50);
        var endDate = TestData.Clock.Today.AddDays(-10);
        var person = await TestData.CreatePersonAsync(b => b.WithAlert(a => a.WithStartDate(startDate).WithEndDate(endDate)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var newEndDate = TestData.Clock.Today.AddDays(-5);
        var reason = AlertChangeEndDateReasonOption.AnotherReason;
        var reasonDetail = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToEditAlertEndDatePageAsync(alertId);

        await page.AssertOnEditAlertEndDatePageAsync(alertId);

        await page.FillDateInputAsync(newEndDate);

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditAlertEndDateChangeReasonPageAsync(alertId);

        await page.Locator("div.govuk-form-group:has-text('Select a reason')").Locator($"label{TextIsSelector(reason.GetDisplayName())}").CheckAsync();
        await page.Locator("div.govuk-form-group:has-text('Do you want to add more information about why you’re changing the end date?')").Locator("label:text-is('Yes')").CheckAsync();
        await page.FillAsync("label:text-is('Add additional detail')", reasonDetail);
        await page.Locator("div.govuk-form-group:has-text('Do you want to upload evidence?')").Locator("label:text-is('Yes')").CheckAsync();
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditAlertEndDateCheckAnswersPageAsync(alertId);

        await page.ClickConfirmChangeButtonAsync();

        await page.AssertOnPersonAlertsPageAsync(personId);

        await page.AssertFlashMessageAsync("Alert changed");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task EditAlertLink(bool hasCurrentLink)
    {
        var startDate = TestData.Clock.Today.AddDays(-50);
        var link = hasCurrentLink ? TestData.GenerateUrl() : null;
        var person = await TestData.CreatePersonAsync(b => b.WithAlert(a => a.WithStartDate(startDate).WithExternalLink(link)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var newLink = TestData.GenerateUrl();
        var changeReason = AlertChangeLinkReasonOption.ChangeOfLink;
        var changeReasonDetail = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToEditAlertLinkPageAsync(alertId);

        await page.AssertOnEditAlertLinkPageAsync(alertId);

        if (hasCurrentLink)
        {
            await page.CheckAsync("label:text-is('Change link')");
        }
        else
        {
            await page.CheckAsync("label:text-is('Yes')");
        }

        await page.FillAsync("label:text-is('Enter link to panel outcome')", newLink);

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditAlertLinkChangeReasonPageAsync(alertId);

        await page.Locator("div.govuk-form-group:has-text('Select a reason')").Locator($"label{TextIsSelector(changeReason.GetDisplayName())}").CheckAsync();
        await page.Locator("div.govuk-form-group:has-text('Do you want to add more information about why you’re changing the panel outcome link?')").Locator("label:text-is('Yes')").CheckAsync();
        await page.FillAsync("label:text-is('Add additional detail')", changeReasonDetail);
        await page.Locator("div.govuk-form-group:has-text('Do you want to upload evidence?')").Locator("label:text-is('Yes')").CheckAsync();
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButtonAsync();

        await page.AssertOnEditAlertLinkCheckAnswersPageAsync(alertId);

        await page.ClickConfirmChangeButtonAsync();

        await page.AssertOnPersonAlertsPageAsync(personId);

        await page.AssertFlashMessageAsync("Alert changed");
    }

    [Fact]
    public async Task CloseAlert()
    {
        var startDate = TestData.Clock.Today.AddDays(-50);
        var person = await TestData.CreatePersonAsync(b => b.WithAlert(a => a.WithStartDate(startDate)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var newEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReason = CloseAlertReasonOption.AlertPeriodHasEnded;
        var changeReasonDetail = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToCloseAlertPageAsync(alertId);

        await page.AssertOnCloseAlertPageAsync(alertId);

        await page.FillDateInputAsync(newEndDate);

        await page.ClickContinueButtonAsync();

        await page.AssertOnCloseAlertChangeReasonPageAsync(alertId);

        await page.Locator("div.govuk-form-group:has-text('Select a reason')").Locator($"label{TextIsSelector(changeReason.GetDisplayName())}").CheckAsync();
        await page.Locator("div.govuk-form-group:has-text('Do you want to add more information about why you’re adding an end date?')").Locator("label:text-is('Yes')").CheckAsync();
        await page.FillAsync("label:text-is('Add additional detail')", changeReasonDetail);
        await page.Locator("div.govuk-form-group:has-text('Do you want to upload evidence?')").Locator("label:text-is('Yes')").CheckAsync();
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButtonAsync();

        await page.AssertOnCloseAlertCheckAnswersPageAsync(alertId);

        await page.ClickButtonAsync("Confirm and close alert");

        await page.AssertOnPersonAlertsPageAsync(personId);

        await page.AssertFlashMessageAsync("Alert closed");
    }

    [Fact]
    public async Task ReopenAlert()
    {
        var startDate = TestData.Clock.Today.AddDays(-50);
        var endDate = TestData.Clock.Today.AddDays(-10);
        var person = await TestData.CreatePersonAsync(b => b.WithAlert(a => a.WithStartDate(startDate).WithEndDate(endDate)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var changeReason = ReopenAlertReasonOption.ClosedInError;
        var changeReasonDetail = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToReopenAlertPageAsync(alertId);

        await page.AssertOnReopenAlertPageAsync(alertId);

        await page.Locator("div.govuk-form-group:has-text('Select a reason')").Locator($"label{TextIsSelector(changeReason.GetDisplayName())}").CheckAsync();
        await page.Locator("div.govuk-form-group:has-text('Do you want to add more information about why you’re removing the end date?')").Locator("label:text-is('Yes')").CheckAsync();
        await page.FillAsync("label:text-is('Add additional detail')", changeReasonDetail);
        await page.Locator("div.govuk-form-group:has-text('Do you want to upload evidence?')").Locator("label:text-is('Yes')").CheckAsync();
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButtonAsync();

        await page.AssertOnReopenAlertCheckAnswersPageAsync(alertId);

        await page.ClickButtonAsync("Confirm and re-open alert");

        await page.AssertOnPersonAlertsPageAsync(personId);

        await page.AssertFlashMessageAsync("Alert re-opened");
    }

    [Fact]
    public async Task DeleteAlert()
    {
        var startDate = TestData.Clock.Today.AddDays(-50);
        var endDate = TestData.Clock.Today.AddDays(-10);
        var person = await TestData.CreatePersonAsync(b => b.WithAlert(a => a.WithStartDate(startDate).WithEndDate(endDate)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var deleteReasonDetails = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToDeleteAlertPageAsync(alertId);

        await page.AssertOnDeleteAlertPageAsync(alertId);

        await page.Locator("div.govuk-form-group:has-text('Do you want to add why you are deleting this alert?')").Locator("label:text-is('Yes')").CheckAsync();
        await page.FillAsync("label:text-is('Add additional detail')", deleteReasonDetails);
        await page.Locator("div.govuk-form-group:has-text('Do you want to upload evidence?')").Locator("label:text-is('Yes')").CheckAsync();
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButtonAsync();

        await page.AssertOnDeleteAlertCheckAnswersPageAsync(alertId);

        await page.ClickButtonAsync("Delete alert");

        await page.AssertOnPersonAlertsPageAsync(personId);

        await page.AssertFlashMessageAsync("Alert deleted");
    }
}
