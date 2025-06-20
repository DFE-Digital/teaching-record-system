namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class ApiTrnRequestSupportTaskTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task CreateNewRecord()
    {
        // Start with a blank slate of tasks
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync();
        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId, t => t.WithStatus(SupportTaskStatus.Open));
        var requestData = supportTask.TrnRequestMetadata!;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/api-trn-requests");

        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches");

        await page.CheckAsync($"label{TextIsSelector($"I want to create a new record from the {applicationUser.Name} request")}");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers");
        await page.ClickButtonAsync("Confirm and merge records");

        await page.WaitForUrlPathAsync("/support-tasks/api-trn-requests");
    }

    [Fact]
    public async Task UpdateExisting()
    {
        // Start with a blank slate of tasks
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var match = await TestData.CreatePersonAsync(p => p.WithTrn());

        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateApiTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedRecords(match.PersonId)
                .WithStatus(SupportTaskStatus.Open)
                .WithFirstName(match.FirstName)
                .WithMiddleName(TestData.GenerateChangedMiddleName(match.MiddleName))
                .WithLastName(match.LastName)
                .WithDateOfBirth(match.DateOfBirth)
                .WithEmailAddress(match.Email)
                .WithNationalInsuranceNumber(match.NationalInsuranceNumber));

        var requestData = supportTask.TrnRequestMetadata!;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/support-tasks/api-trn-requests");

        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/matches");

        await page.CheckAsync("label:text-is('Record A')");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/merge");
        await page.CheckAsync($"label{TextIsSelector(requestData.MiddleName)}");
        await page.ClickContinueButtonAsync();

        await page.WaitForUrlPathAsync($"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/check-answers");
        await page.ClickButtonAsync("Confirm and merge records");

        await page.WaitForUrlPathAsync("/support-tasks/api-trn-requests");
    }
}

