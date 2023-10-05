namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class AlertTests : TestBase
{
    public AlertTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task CloseAlert()
    {
        var startDate = new DateOnly(2021, 10, 01);
        var endDate = new DateOnly(2023, 08, 02);
        var createPersonResult = await TestData.CreatePerson(b => b.WithSanction("G1", startDate: startDate));
        var personId = createPersonResult.ContactId;
        var alertId = createPersonResult.Sanctions.Single().SanctionId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonAlertsPage(personId);

        await page.AssertOnPersonAlertsPage(personId);

        await page.ClickCloseAlertPersonAlertsPage(alertId);

        await page.AssertOnCloseAlertPage(alertId);

        await page.FillDateInput(endDate);

        await page.ClickContinueButton();

        await page.AssertOnCloseAlertConfirmPage(alertId);

        await page.ClickConfirmButton();

        await page.AssertOnPersonAlertsPage(personId);

        await page.AssertFlashMessage("Alert closed");
    }

    [Theory]
    [InlineData(true, "Status changed to inactive")]
    [InlineData(false, "Inactive status removed")]
    public async Task ViewAlert(bool isActive, string expectedFlashMessage)
    {
        var startDate = new DateOnly(2021, 10, 01);
        var endDate = new DateOnly(2023, 08, 02);
        var createPersonResult = await TestData.CreatePerson(b => b.WithSanction("G1", startDate: startDate, endDate: endDate, spent: true, isActive: isActive));
        var personId = createPersonResult.ContactId;
        var alertId = createPersonResult.Sanctions.Single().SanctionId;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToPersonAlertsPage(personId);

        await page.AssertOnPersonAlertsPage(personId);

        await page.ClickViewAlertPersonAlertsPage(alertId);

        await page.AssertOnAlertDetailPage(alertId);

        if (isActive)
        {
            await page.ClickDeactivateButton();
        }
        else
        {
            await page.ClickReactivateButton();
        }

        await page.AssertOnAlertDetailPage(alertId);

        await page.AssertFlashMessage(expectedFlashMessage);
    }
}
