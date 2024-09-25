using Microsoft.Playwright;

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
        var endDate = new DateOnly(2023, 2, 10);
        var reason = TestData.GenerateLoremIpsum();
        var evidenceFileName = "evidence.jpg";
        var evidenceFileMimeType = "image/jpeg";
        var personId = person.PersonId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAddAlertPage(personId);

        await page.AssertOnAddAlertTypePage();

        await page.CheckAsync($"text={alertType.Name}");

        await page.ClickContinueButton();

        await page.AssertOnAddAlertDetailsPage();

        await page.FillAsync("label:text-is('Enter details')", details);

        await page.ClickContinueButton();

        await page.AssertOnAddAlertLinkPage();

        await page.FillAsync("label:text-is('Enter link')", link);

        await page.ClickContinueButton();

        await page.AssertOnAddAlertStartDatePage();

        await page.FillDateInput(startDate);

        await page.ClickContinueButton();

        await page.AssertOnAddAlertEndDatePage();

        await page.CheckAsync("text=Yes");
        await page.FillDateInput(endDate);

        await page.ClickContinueButton();

        await page.AssertOnAddAlertReasonPage();

        await page.FillAsync("label:text-is('Why are you adding this alert?')", reason);

        await page.CheckAsync("text=Yes");
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

        await page.FillDateInput(startDate);

        await page.ClickContinueButton();

        await page.AssertOnEditAlertStartDateChangeReasonPage(alertId);

        await page.CheckAsync("text=Another reason");
        await page.FillAsync("label:text-is('Enter details')", changeReason);

        await page.CheckAsync("text=Yes");
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
}
