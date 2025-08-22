using TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;
using TeachingRecordSystem.SupportUi.Pages.Common;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class NpqTrnRequestTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Resolve_CreateNewRecord()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest).ExecuteDeleteAsync());

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
        await page.ClickChangeLinkAsync();

        await page.AssertOnMatchesPageAsync(supportTaskReference);
        await page.ClickBackLinkAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickChangeLinkAsync();

        await page.AssertOnMatchesPageAsync(supportTaskReference);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickButtonAsync("Confirm and create record");
        await page.AssertOnListPageAsync();
        await page.AssertBannerAsync("Success", "NPQ request completed");

        await page.FollowBannerLink($"Record created for {requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");
        page.AssertOnAPersonDetailPageAsync();
    }

    [Fact]
    public async Task Resolve_MergeWithExistingRecord_FieldValuesMatchExisting()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest).ExecuteDeleteAsync());

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
                t.WithMatchedPersons([matchedPerson1.PersonId, matchedPerson2.PersonId]);
                t.WithNationalInsuranceNumber(matchedPerson1.NationalInsuranceNumber);
                t.WithDateOfBirth(matchedPerson1.DateOfBirth);
                t.WithEmailAddress(matchedPerson1.Email!);
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
        await page.ClickChangeLinkAsync();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickBackLinkAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickChangeLinkAsync();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickButtonAsync("Confirm and update existing record");
        await page.AssertOnListPageAsync();
        await page.AssertBannerAsync("Success", "NPQ request completed");

        await page.FollowBannerLink($"Record updated for {matchedPerson1.FirstName} {matchedPerson1.MiddleName} {matchedPerson1.LastName}");
        await page.AssertOnPersonDetailPageAsync(matchedPerson1.PersonId);
    }

    [Fact]
    public async Task Resolve_MergeWithExistingRecord_FieldValuesDifferFromExisting_SelectExistingRecordData()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest).ExecuteDeleteAsync());

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
                t.WithMatchedPersons([matchedPerson1.PersonId, matchedPerson2.PersonId]);
                t.WithNationalInsuranceNumber(matchedPerson1.NationalInsuranceNumber);
                t.WithDateOfBirth(matchedPerson1.DateOfBirth);
                t.WithEmailAddress(matchedPerson1.Email!);
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
        await page.AssertContentEqualsAsync(matchedPerson2.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(matchedPerson2.Email!, "Email");
        await page.AssertContentEqualsAsync(matchedPerson2.NationalInsuranceNumber!, "National Insurance number");
        await page.ClickChangeLinkAsync();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickBackLinkAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickChangeLinkAsync();

        await page.AssertOnMergePageAsync(supportTaskReference);
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickButtonAsync("Confirm and update existing record");
        await page.AssertOnListPageAsync();
        await page.AssertBannerAsync("Success", "NPQ request completed");

        await page.FollowBannerLink($"Record updated for {matchedPerson2.FirstName} {matchedPerson2.MiddleName} {matchedPerson2.LastName}");
        await page.AssertOnPersonDetailPageAsync(matchedPerson2.PersonId);
    }

    [Fact]
    public async Task Resolve_MergeWithExistingRecord_FieldValuesDifferFromExisting_SelectSupportRequestData()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest).ExecuteDeleteAsync());

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
                t.WithMatchedPersons([matchedPerson1.PersonId, matchedPerson2.PersonId]);
                t.WithNationalInsuranceNumber(matchedPerson1.NationalInsuranceNumber);
                t.WithDateOfBirth(matchedPerson1.DateOfBirth);
                t.WithEmailAddress(matchedPerson1.Email!);
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
        await page.AssertContentEqualsAsync(requestData.DateOfBirth.ToString(UiDefaults.DateOnlyDisplayFormat), "Date of birth");
        await page.AssertContentEqualsAsync(requestData.EmailAddress!, "Email");
        await page.AssertContentEqualsAsync(requestData.NationalInsuranceNumber!, "National Insurance number");
    }

    [Fact]
    public async Task NoMatches_CreateRecord()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
        {
            configure.WithMatches(false);
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

        await page.AssertOnNoMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickBackLinkAsync();

        await page.AssertOnDetailsPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync("Yes"); // select create new record (as opposed to reject request)
        await page.ClickContinueButtonAsync();

        await page.AssertOnNoMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickButtonAsync("Confirm and create record");

        await page.AssertOnListPageAsync();
        await page.AssertBannerAsync("Success", "NPQ request completed");

        await page.FollowBannerLink($"Record created for {requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");
        page.AssertOnAPersonDetailPageAsync();
    }

    [Fact]
    public async Task RejectRequest()
    {
        await WithDbContext(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, configure =>
            configure.WithMatches(false).WithMiddleName(TestData.GenerateMiddleName()));

        var requestData = supportTask.TrnRequestMetadata!;
        var supportTaskReference = supportTask.SupportTaskReference;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GotoAsync("/support-tasks/npq-trn-requests");

        await page.AssertOnListPageAsync();
        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.AssertOnDetailsPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync("No, reject this request");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRejectionReasonPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync("NPQ details do not match");
        await page.ClickContinueButtonAsync();

        await page.AssertOnRejectCheckYourAnswersPageAsync(supportTaskReference);
        await page.AssertContentContainsAsync("NPQ details do not match", "Reason");
        await page.ClickChangeLinkAsync();

        await page.AssertOnRejectionReasonPageAsync(supportTaskReference);
        await page.ClickBackLinkAsync();

        await page.AssertOnRejectCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickChangeLinkAsync();

        await page.AssertOnRejectionReasonPageAsync(supportTaskReference);
        await page.ClickContinueButtonAsync();

        await page.AssertOnRejectCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickButtonAsync("Confirm and reject request");

        await page.AssertOnListPageAsync();
        await page.AssertBannerAsync("Success", $"TRN request for {requestData.FirstName} {requestData.MiddleName} {requestData.LastName} rejected");
    }
}
