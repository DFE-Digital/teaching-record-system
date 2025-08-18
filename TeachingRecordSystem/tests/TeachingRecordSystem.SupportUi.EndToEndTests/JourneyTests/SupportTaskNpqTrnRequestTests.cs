using TeachingRecordSystem.SupportUi.EndToEndTests.NpqTrnRequests;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class SupportTaskNpqTrnRequestTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task CreateNewRecord_RequestJourney()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);
        var requestData = supportTask.TrnRequestMetadata!;
        var supportTaskReference = supportTask.SupportTaskReference;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GotoAsync("/support-tasks/npq-trn-requests");

        await page.AssertOnListPageAsync();
        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.AssertOnDetailsPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync("Yes"); // select create new record (as opposed to reject request)
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync($"I want to create a new record from the {applicationUser.Name} request");
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickChangeLink();

        await page.AssertOnMatchesPageAsync(supportTaskReference);
        await page.ClickBackLink();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickChangeLink();

        await page.AssertOnMatchesPageAsync(supportTaskReference);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickButtonAsync("Confirm and create record");
        await page.AssertOnListPageAsync();
        await page.AssertSuccessBannerAsync($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");

        await page.FollowBannerLink();
        page.AssertOnAPersonDetailPage();
    }

    [Fact]
    public async Task MergeWithExistingRecord_AllFieldValuesMatchExisting_RequestJourney()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);
        var requestData = supportTask.TrnRequestMetadata!;
        var supportTaskReference = supportTask.SupportTaskReference;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GotoAsync("/support-tasks/npq-trn-requests");

        await page.AssertOnListPageAsync();
        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.AssertOnDetailsPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync("Yes"); // select create new record (as opposed to reject request)
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync($"Record A");
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickChangeLink();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickBackLink();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickChangeLink();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickButtonAsync("Confirm and update existing record");
        await page.AssertOnListPageAsync();
        await page.AssertSuccessBannerAsync($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");

        await page.FollowBannerLink();
        page.AssertOnAPersonDetailPage();
    }

    [Fact]
    public async Task MergeWithExistingRecord_SomeFieldValuesDifferInExisting_RequestJourney()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        // CML todo - Set up two records to merge with, where one has no conflicting info, 1 does have known conflictin info
        // select the conflicting info one
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);
        var requestData = supportTask.TrnRequestMetadata!;
        var supportTaskReference = supportTask.SupportTaskReference;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GotoAsync("/support-tasks/npq-trn-requests");

        await page.AssertOnListPageAsync();
        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.AssertOnDetailsPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync("Yes"); // select create new record (as opposed to reject request)
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync($"Record B");
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickChangeLink();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickBackLink();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickChangeLink();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickButtonAsync("Confirm and update existing record");
        await page.AssertOnListPageAsync();
        await page.AssertSuccessBannerAsync($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");

        await page.FollowBannerLink();
        page.AssertOnAPersonDetailPage();
    }
}
