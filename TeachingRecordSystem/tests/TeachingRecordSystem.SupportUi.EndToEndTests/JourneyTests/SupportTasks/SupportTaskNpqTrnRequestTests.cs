using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

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
    public async Task MergeWithExistingRecord_FieldValuesMatchExisting_RequestJourney()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        // Set up two records to merge with, where one has no conflicting info, one does have known conflicting info
        var matchedPerson1 = await TestData.CreatePersonAsync(p =>
        {
            p.WithTrn();
            p.WithEmail(TestData.GenerateUniqueEmail());
            p.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var matchedPerson2 = await TestData.CreatePersonAsync(p =>
        {
            p.WithTrn();
            p.WithEmail(TestData.GenerateUniqueEmail());
            p.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t =>
            {
                t.WithMatchedPersons(new Guid[] { matchedPerson1.PersonId, matchedPerson2.PersonId });
                t.WithNationalInsuranceNumber(matchedPerson1.NationalInsuranceNumber);
                t.WithDateOfBirth(matchedPerson1.DateOfBirth);
                t.WithEmailAddress(matchedPerson1.Email);
            });
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
        await page.ClickRadioByLabelAsync($"Record A"); // the record with no conflicting info
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
    public async Task MergeWithExistingRecord_FieldValuesDifferFromExisting_SelectExistingRecordData_RequestJourney()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        // Set up two records to merge with, where one has no conflicting info, one does have known conflicting info
        var matchedPerson1 = await TestData.CreatePersonAsync(p =>
            {
                p.WithTrn();
                p.WithEmail(TestData.GenerateUniqueEmail());
                p.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
            });
        var matchedPerson2 = await TestData.CreatePersonAsync(p =>
        {
            p.WithTrn();
            p.WithEmail(TestData.GenerateUniqueEmail());
            p.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t =>
            {
                t.WithMatchedPersons(new Guid[] { matchedPerson1.PersonId, matchedPerson2.PersonId });
                t.WithNationalInsuranceNumber(matchedPerson1.NationalInsuranceNumber);
                t.WithDateOfBirth(matchedPerson1.DateOfBirth);
                t.WithEmailAddress(matchedPerson1.Email);
            });

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
        await page.ClickRadioByLabelAsync($"Record B"); // select the record with conflicting info
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync(matchedPerson2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(matchedPerson2.Email!);
        await page.ClickRadioByLabelAsync(matchedPerson2.NationalInsuranceNumber!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.AssertContentEquals(matchedPerson2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEquals(matchedPerson2.Email!, "Email");
        await page.AssertContentEquals(matchedPerson2.NationalInsuranceNumber!, "National Insurance number");
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
    public async Task MergeWithExistingRecord_FieldValuesDifferFromExisting_SelectSupportRequestData_RequestJourney()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.ApiTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        // Set up two records to merge with, where one has no conflicting info, one does have known conflicting info
        var matchedPerson1 = await TestData.CreatePersonAsync(p =>
        {
            p.WithTrn();
            p.WithEmail(TestData.GenerateUniqueEmail());
            p.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var matchedPerson2 = await TestData.CreatePersonAsync(p =>
        {
            p.WithTrn();
            p.WithEmail(TestData.GenerateUniqueEmail());
            p.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t =>
            {
                t.WithMatchedPersons(new Guid[] { matchedPerson1.PersonId, matchedPerson2.PersonId });
                t.WithNationalInsuranceNumber(matchedPerson1.NationalInsuranceNumber);
                t.WithDateOfBirth(matchedPerson1.DateOfBirth);
                t.WithEmailAddress(matchedPerson1.Email);
            });

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
        await page.ClickRadioByLabelAsync($"Record B"); // select the record with conflicting info
        await page.ClickContinueButtonAsync();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync(requestData.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat));
        await page.ClickRadioByLabelAsync(requestData.EmailAddress!);
        await page.ClickRadioByLabelAsync(requestData.NationalInsuranceNumber!);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.AssertContentEquals(requestData.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEquals(requestData.EmailAddress!, "Email");
        await page.AssertContentEquals(requestData.NationalInsuranceNumber!, "National Insurance number");
    }
}
