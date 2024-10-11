using Microsoft.Playwright;
using TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class AlertTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task AddAlert()
    {
        var person = await TestData.CreatePerson();
        var alertType = await TestData.ReferenceDataCache.GetAlertTypeById(Guid.Parse("ed0cd700-3fb2-4db0-9403-ba57126090ed")); // Prohibition by the Secretary of State - misconduct
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

        await page.GoToPersonAlertsPage(personId);

        await page.ClickButton("Add an alert");

        await page.AssertOnAddAlertTypePage();

        await page.CheckAsync($"label:text-is('{alertType.Name}')");

        await page.ClickContinueButton();

        await page.AssertOnAddAlertDetailsPage();

        await page.FillAsync($"label:text-is('Enter details about the alert type: {alertType.Name}')", details);

        await page.ClickContinueButton();

        await page.AssertOnAddAlertLinkPage();

        await page.CheckAsync("label:text-is('Yes')");

        await page.FillAsync("label:text-is('Enter link to panel outcome')", link);

        await page.ClickContinueButton();

        await page.AssertOnAddAlertStartDatePage();

        await page.FillDateInput(startDate);

        await page.ClickContinueButton();

        await page.AssertOnAddAlertReasonPage();

        await page.Locator("div.govuk-form-group:has-text('Select a reason')").Locator($"label:text-is('{reason.GetDisplayName()}')").CheckAsync();
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

        await page.ClickContinueButton();

        await page.AssertOnAddAlertCheckAnswersPage();

        await page.ClickButton("Add alert");

        await page.AssertOnPersonAlertsPage(personId);

        await page.AssertFlashMessage("Alert added");
    }

    [Fact]
    public async Task EditAlertStartDate()
    {
        var startDate = new DateOnly(2023, 1, 1);
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithStartDate(startDate)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var newStartDate = new DateOnly(2023, 2, 3);
        var changeReason = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToEditAlertStartDatePage(alertId);

        await page.AssertOnEditAlertStartDatePage(alertId);

        await page.FillDateInput(newStartDate);

        await page.ClickContinueButton();

        await page.AssertOnEditAlertStartDateChangeReasonPage(alertId);

        await page.CheckAsync("text=Another reason");
        await page.FillAsync("label:text-is('Enter details')", changeReason);

        await page.CheckAsync("label:text-is('Yes')");
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButton();

        await page.AssertOnEditAlertStartDateCheckAnswersPage(alertId);

        await page.ClickConfirmChangeButton();

        await page.AssertOnPersonAlertsPage(personId);

        await page.AssertFlashMessage("Alert changed");
    }

    [Fact]
    public async Task EditAlertEndDate()
    {
        var startDate = TestData.Clock.Today.AddDays(-50);
        var endDate = TestData.Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithStartDate(startDate).WithEndDate(endDate)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var newEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReason = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToEditAlertEndDatePage(alertId);

        await page.AssertOnEditAlertEndDatePage(alertId);

        await page.FillDateInput(newEndDate);

        await page.ClickContinueButton();

        await page.AssertOnEditAlertEndDateChangeReasonPage(alertId);

        await page.CheckAsync("label:text-is('Another reason')");
        await page.FillAsync("label:text-is('Enter details')", changeReason);

        await page.CheckAsync("label:text-is('Yes')");
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButton();

        await page.AssertOnEditAlertEndDateCheckAnswersPage(alertId);

        await page.ClickConfirmChangeButton();

        await page.AssertOnPersonAlertsPage(personId);

        await page.AssertFlashMessage("Alert changed");
    }

    [Fact]
    public async Task CloseAlert()
    {
        var startDate = TestData.Clock.Today.AddDays(-50);
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithStartDate(startDate)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var newEndDate = TestData.Clock.Today.AddDays(-5);
        var changeReason = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToCloseAlertPage(alertId);

        await page.AssertOnCloseAlertPage(alertId);

        await page.FillDateInput(newEndDate);

        await page.ClickContinueButton();

        await page.AssertOnCloseAlertChangeReasonPage(alertId);

        await page.CheckAsync("label:text-is('Another reason')");
        await page.FillAsync("label:text-is('Enter details')", changeReason);

        await page.CheckAsync("label:text-is('Yes')");
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButton();

        await page.AssertOnCloseAlertCheckAnswersPage(alertId);

        await page.ClickButton("Confirm and close alert");

        await page.AssertOnPersonAlertsPage(personId);

        await page.AssertFlashMessage("Alert closed");
    }

    [Fact]
    public async Task ReopenAlert()
    {
        var startDate = TestData.Clock.Today.AddDays(-50);
        var endDate = TestData.Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithStartDate(startDate).WithEndDate(endDate)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var changeReason = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToReopenAlertPage(alertId);

        await page.AssertOnReopenAlertPage(alertId);

        await page.CheckAsync("label:text-is('Another reason')");
        await page.FillAsync("label:text-is('Enter details')", changeReason);

        await page.CheckAsync("label:text-is('Yes')");
        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickContinueButton();

        await page.AssertOnReopenAlertCheckAnswersPage(alertId);

        await page.ClickButton("Confirm and re-open alert");

        await page.AssertOnPersonAlertsPage(personId);

        await page.AssertFlashMessage("Alert re-opened");
    }

    [Fact]
    public async Task DeleteAlert()
    {
        var startDate = TestData.Clock.Today.AddDays(-50);
        var endDate = TestData.Clock.Today.AddDays(-10);
        var person = await TestData.CreatePerson(b => b.WithAlert(a => a.WithStartDate(startDate).WithEndDate(endDate)));
        var personId = person.PersonId;
        var alertId = person.Alerts.First().AlertId;
        var deleteReason = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToDeleteAlertPage(alertId);

        await page.AssertOnDeleteAlertPage(alertId);
        await page.CheckAsync("label:text-is('Yes, I want to delete this alert')");

        await page.ClickContinueButton();

        await page.AssertOnDeleteAlertConfirmPage(alertId);

        await page.CheckAsync(":nth-match(label:text-is('Yes'), 1)");

        await page.FillAsync("label:text-is('Add additional detail')", deleteReason);

        await page.CheckAsync(":nth-match(label:text-is('Yes'), 2)");

        await page
            .GetByLabel("Upload a file")
            .SetInputFilesAsync(
                new FilePayload()
                {
                    Name = evidenceFileName,
                    MimeType = evidenceFileMimeType,
                    Buffer = TestData.JpegImage
                });

        await page.ClickButton("Delete this alert");

        await page.AssertOnPersonAlertsPage(personId);

        await page.AssertFlashMessage("Alert deleted");
    }
}
