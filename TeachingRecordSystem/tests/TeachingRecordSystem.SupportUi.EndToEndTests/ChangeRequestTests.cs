using TeachingRecordSystem.SupportUi.EndToEndTests.Shared;

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
        var createPersonResult = await TestData.CreatePersonAsync();
        string caseReference;
        if (isNameChange)
        {
            var createIncidentResult = await TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }
        else
        {
            var createIncidentResult = await TestData.CreateDateOfBirthChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToHomePageAsync();

        await page.ClickSupportTasksLinkInNavigationBarAsync();

        await page.AssertOnSupportTasksPageAsync();

        await page.ClickCaseReferenceLinkChangeRequestsPageAsync(caseReference);

        await page.AssertOnChangeRequestDetailPageAsync(caseReference);

        await page.ClickAcceptChangeButtonAsync();

        await page.AssertOnAcceptChangeRequestPageAsync(caseReference);

        await page.ClickConfirmButtonAsync();

        await page.AssertOnSupportTasksPageAsync();

        await page.AssertFlashMessageAsync("The request has been accepted");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndReject(bool isNameChange)
    {
        var createPersonResult = await TestData.CreatePersonAsync();
        string caseReference;
        if (isNameChange)
        {
            var createIncidentResult = await TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }
        else
        {
            var createIncidentResult = await TestData.CreateDateOfBirthChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToHomePageAsync();

        await page.ClickSupportTasksLinkInNavigationBarAsync();

        await page.AssertOnSupportTasksPageAsync();

        await page.ClickCaseReferenceLinkChangeRequestsPageAsync(caseReference);

        await page.AssertOnChangeRequestDetailPageAsync(caseReference);

        await page.ClickRejectChangeButtonAsync();

        await page.AssertOnRejectChangeRequestPageAsync(caseReference);

        await page.CheckAsync("label:text-is('Request and proof don’t match')");

        await page.ClickRejectButtonAsync();

        await page.AssertOnSupportTasksPageAsync();

        await page.AssertFlashMessageAsync("The request has been rejected");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndCancel(bool isNameChange)
    {
        var createPersonResult = await TestData.CreatePersonAsync();
        string caseReference;
        if (isNameChange)
        {
            var createIncidentResult = await TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }
        else
        {
            var createIncidentResult = await TestData.CreateDateOfBirthChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));
            caseReference = createIncidentResult.TicketNumber;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToHomePageAsync();

        await page.ClickSupportTasksLinkInNavigationBarAsync();

        await page.AssertOnSupportTasksPageAsync();

        await page.ClickCaseReferenceLinkChangeRequestsPageAsync(caseReference);

        await page.AssertOnChangeRequestDetailPageAsync(caseReference);

        await page.ClickRejectChangeButtonAsync();

        await page.AssertOnRejectChangeRequestPageAsync(caseReference);

        await page.CheckAsync("label:text-is('Change no longer required')");

        await page.ClickRejectButtonAsync();

        await page.AssertOnSupportTasksPageAsync();

        await page.AssertFlashMessageAsync("The request has been cancelled");
    }
}
