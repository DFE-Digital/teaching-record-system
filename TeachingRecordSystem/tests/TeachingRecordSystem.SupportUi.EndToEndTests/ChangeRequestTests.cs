namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class ChangeRequestTests : TestBase
{
    public ChangeRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndApprove(bool isNameChange)
    {
        var createPersonResult = await TestData.CreatePerson();
        string caseReference;
        if (isNameChange)
        {
            var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }
        else
        {
            var createIncidentResult = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToHomePage();

        await page.ClickChangeRequestsLinkInNavigationBar();

        await page.AssertOnChangeRequestsPage();

        await page.ClickCaseReferenceLinkChangeRequestsPage(caseReference);

        await page.AssertOnChangeRequestDetailPage(caseReference);

        await page.ClickAcceptChangeButton();

        await page.AssertOnAcceptChangeRequestPage(caseReference);

        await page.ClickConfirmButton();

        await page.AssertOnChangeRequestsPage();

        await page.AssertFlashMessage("The request has been accepted");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndReject(bool isNameChange)
    {
        var createPersonResult = await TestData.CreatePerson();
        string caseReference;
        if (isNameChange)
        {
            var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }
        else
        {
            var createIncidentResult = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToHomePage();

        await page.ClickChangeRequestsLinkInNavigationBar();

        await page.AssertOnChangeRequestsPage();

        await page.ClickCaseReferenceLinkChangeRequestsPage(caseReference);

        await page.AssertOnChangeRequestDetailPage(caseReference);

        await page.ClickRejectChangeButton();

        await page.AssertOnRejectChangeRequestPage(caseReference);

        await page.CheckAsync("label:text-is('Request and proof don’t match')");

        await page.ClickRejectButton();

        await page.AssertOnChangeRequestsPage();

        await page.AssertFlashMessage("The request has been rejected");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndCancel(bool isNameChange)
    {
        var createPersonResult = await TestData.CreatePerson();
        string caseReference;
        if (isNameChange)
        {
            var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }
        else
        {
            var createIncidentResult = await TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToHomePage();

        await page.ClickChangeRequestsLinkInNavigationBar();

        await page.AssertOnChangeRequestsPage();

        await page.ClickCaseReferenceLinkChangeRequestsPage(caseReference);

        await page.AssertOnChangeRequestDetailPage(caseReference);

        await page.ClickRejectChangeButton();

        await page.AssertOnRejectChangeRequestPage(caseReference);

        await page.CheckAsync("label:text-is('Change no longer required')");

        await page.ClickRejectButton();

        await page.AssertOnChangeRequestsPage();

        await page.AssertFlashMessage("The request has been cancelled");
    }
}
