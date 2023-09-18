using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class CaseTests : TestBase
{
    public CaseTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task SelectCaseAndApprove()
    {
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
        var caseReference = createIncidentResult.TicketNumber;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToHomePage();

        await page.ClickOpenCasesLinkInNavigationBar();

        await page.AssertOnOpenCasesPage();

        await page.ClickCaseReferenceLinkOpenCasesPage(caseReference);

        await page.AssertOnCaseDetailPage(caseReference);

        await page.ClickAcceptChangeButton();

        await page.AssertOnAcceptCasePage(caseReference);

        await page.ClickConfirmButton();

        await page.AssertOnOpenCasesPage();

        await page.AssertFlashMessage("The request has been accepted");
    }

    [Fact]
    public async Task SelectCaseAndReject()
    {
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
        var caseReference = createIncidentResult.TicketNumber;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToHomePage();

        await page.ClickOpenCasesLinkInNavigationBar();

        await page.AssertOnOpenCasesPage();

        await page.ClickCaseReferenceLinkOpenCasesPage(caseReference);

        await page.AssertOnCaseDetailPage(caseReference);

        await page.ClickRejectChangeButton();

        await page.AssertOnRejectCasePage(caseReference);

        await page.CheckAsync("label:text-is('Request and proof don’t match')");

        await page.ClickConfirmButton();

        await page.AssertOnOpenCasesPage();

        await page.AssertFlashMessage("The request has been rejected");
    }

    [Fact]
    public async Task SelectCaseAndCancel()
    {
        var createPersonResult = await TestData.CreatePerson();
        var createIncidentResult = await TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
        var caseReference = createIncidentResult.TicketNumber;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToHomePage();

        await page.ClickOpenCasesLinkInNavigationBar();

        await page.AssertOnOpenCasesPage();

        await page.ClickCaseReferenceLinkOpenCasesPage(caseReference);

        await page.AssertOnCaseDetailPage(caseReference);

        await page.ClickRejectChangeButton();

        await page.AssertOnRejectCasePage(caseReference);

        await page.CheckAsync("label:text-is('Change no longer required')");

        await page.ClickConfirmButton();

        await page.AssertOnOpenCasesPage();

        await page.AssertFlashMessage("The request has been cancelled");
    }
}
