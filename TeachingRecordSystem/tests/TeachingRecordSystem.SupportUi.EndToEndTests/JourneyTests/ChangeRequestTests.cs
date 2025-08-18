namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class ChangeRequestTests : TestBase
{
    public ChangeRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Theory(Skip = "Will re-enable once Support Tasks Index and Accept page has been changed to use TRS support task")]
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

        await page.GotoAsync("/support-tasks");

        await page.AssertOnSupportTasksPageAsync();

        await page.ClickCaseReferenceLinkChangeRequestsPageAsync(caseReference);

        await page.AssertOnChangeRequestDetailPageAsync(caseReference);

        await page.ClickAcceptChangeButtonAsync();

        await page.AssertOnAcceptChangeRequestPageAsync(caseReference);

        await page.ClickConfirmButtonAsync();

        await page.AssertOnSupportTasksPageAsync();

        await page.AssertFlashMessageAsync("The request has been accepted");
    }

    [Theory(Skip = "Will re-enable once Support Tasks Index and Reject page has been changed to use TRS support task")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndReject(bool isNameChange)
    {
        var createPersonResult = await TestData.CreatePersonAsync();
        string supportTaskReference;
        if (isNameChange)
        {
            var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));
            supportTaskReference = supportTask.SupportTaskReference;
        }
        else
        {
            var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth)));
            supportTaskReference = supportTask.SupportTaskReference;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks");

        await page.AssertOnSupportTasksPageAsync();

        await page.ClickCaseReferenceLinkChangeRequestsPageAsync(supportTaskReference);

        await page.AssertOnChangeRequestDetailPageAsync(supportTaskReference);

        await page.ClickRejectChangeButtonAsync();

        await page.AssertOnRejectChangeRequestPageAsync(supportTaskReference);

        await page.CheckAsync("label:text-is('Request and proof don’t match')");

        await page.ClickRejectButtonAsync();

        await page.AssertOnSupportTasksPageAsync();

        await page.AssertFlashMessageAsync("The request has been rejected");
    }

    [Theory(Skip = "Will re-enable once Support Tasks Index and Reject page has been changed to use TRS support task")]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndCancel(bool isNameChange)
    {
        var createPersonResult = await TestData.CreatePersonAsync();
        string supportTaskReference;
        if (isNameChange)
        {
            var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName(createPersonResult.LastName)));
            supportTaskReference = supportTask.SupportTaskReference;
        }
        else
        {
            var supportTask = await TestData.CreateChangeDateOfBirthRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithDateOfBirth(TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth)));
            supportTaskReference = supportTask.SupportTaskReference;
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks");

        await page.ClickCaseReferenceLinkChangeRequestsPageAsync(supportTaskReference);

        await page.AssertOnChangeRequestDetailPageAsync(supportTaskReference);

        await page.ClickRejectChangeButtonAsync();

        await page.AssertOnRejectChangeRequestPageAsync(supportTaskReference);

        await page.CheckAsync("label:text-is('Change no longer required')");

        await page.ClickRejectButtonAsync();

        await page.AssertOnSupportTasksPageAsync();

        await page.AssertFlashMessageAsync("The request has been cancelled");
    }
}
