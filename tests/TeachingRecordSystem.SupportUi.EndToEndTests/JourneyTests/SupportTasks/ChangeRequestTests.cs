namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class ChangeRequestTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SelectChangeRequestAndApprove(bool isNameChange)
    {
        var createPersonResult = await TestData.CreatePersonAsync();
        string supportTaskReference;
        if (isNameChange)
        {
            var supportTask = await TestData.CreateChangeNameRequestSupportTaskAsync(
                createPersonResult.PersonId,
                b => b.WithLastName(TestData.GenerateChangedLastName([createPersonResult.FirstName, createPersonResult.MiddleName, createPersonResult.LastName])));
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

        await page.GotoAsync("/support-tasks/change-requests");

        await page.AssertOnChangeRequestsPageAsync();

        await page.ClickAsync($"a{TextIsSelector($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}")}");

        await page.AssertOnChangeRequestDetailPageAsync(supportTaskReference);

        await page.ClickAcceptChangeButtonAsync();

        await page.AssertOnAcceptChangeRequestPageAsync(supportTaskReference);

        await page.ClickConfirmButtonAsync();

        await page.AssertOnChangeRequestsPageAsync();

        await page.AssertFlashMessageAsync("The request has been accepted");
    }

    [Theory]
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
                b => b.WithLastName(TestData.GenerateChangedLastName([createPersonResult.FirstName, createPersonResult.MiddleName, createPersonResult.LastName])));
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

        await page.GotoAsync("/support-tasks/change-requests");

        await page.AssertOnChangeRequestsPageAsync();

        await page.ClickAsync($"a{TextIsSelector($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}")}");

        await page.AssertOnChangeRequestDetailPageAsync(supportTaskReference);

        await page.ClickRejectChangeButtonAsync();

        await page.AssertOnRejectChangeRequestPageAsync(supportTaskReference);

        await page.CheckAsync("label:text-is('Request and proof don’t match')");

        await page.ClickRejectButtonAsync();

        await page.AssertOnChangeRequestsPageAsync();

        await page.AssertFlashMessageAsync("The request has been rejected");
    }

    [Theory]
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
                b => b.WithLastName(TestData.GenerateChangedLastName([createPersonResult.FirstName, createPersonResult.MiddleName, createPersonResult.LastName])));
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

        await page.GotoAsync("/support-tasks/change-requests");

        await page.ClickAsync($"a{TextIsSelector($"{createPersonResult.FirstName} {createPersonResult.MiddleName} {createPersonResult.LastName}")}");

        await page.AssertOnChangeRequestDetailPageAsync(supportTaskReference);

        await page.ClickRejectChangeButtonAsync();

        await page.AssertOnRejectChangeRequestPageAsync(supportTaskReference);

        await page.CheckAsync("label:text-is('Change no longer required')");

        await page.ClickRejectButtonAsync();

        await page.AssertOnChangeRequestsPageAsync();

        await page.AssertFlashMessageAsync("The request has been cancelled");
    }
}
