namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.SupportTasks;

public class NpqTrnRequestTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Resolve_CreateNewRecord()
    {
        await WithDbContextAsync(dbContext =>
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
        await page.ClickRadioByLabelAsync($"Create a new record");
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickBackLinkAsync();
        await page.AssertOnMatchesPageAsync(supportTaskReference);

        await page.ClickRadioByLabelAsync($"Create a new record");
        await page.ClickContinueButtonAsync();
        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickButtonAsync("Confirm and create record");
        await page.AssertOnListPageAsync();
        await page.AssertBannerAsync("Success", $"TRN request for {StringHelper.JoinNonEmpty(' ', requestData.FirstName, requestData.MiddleName, requestData.LastName)} completed");

        await page.AssertBannerLinksToPersonRecord();
    }

    [Fact]
    public async Task Resolve_MultiplePotentialMatches_MergeWithExistingRecord()
    {
        await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        // Set up two potential matched records to merge with
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);
        var requestData = supportTask.TrnRequestMetadata!;
        var supportTaskReference = supportTask.SupportTaskReference;
        var matchedPersonA = await WithDbContextAsync(dbContext =>
            dbContext.Persons.SingleAsync(p => p.PersonId == supportTask.TrnRequestMetadata!.Matches!.MatchedPersons[0].PersonId));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();
        await page.GotoAsync("/support-tasks/npq-trn-requests");

        await page.AssertOnListPageAsync();
        await page.ClickAsync($"a{TextIsSelector($"{requestData.FirstName} {requestData.MiddleName} {requestData.LastName}")}");

        await page.AssertOnDetailsPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync("Yes"); // select create new record (as opposed to reject request)
        await page.ClickContinueButtonAsync();

        await page.AssertOnMatchesPageAsync(supportTaskReference);
        await page.ClickRadioByLabelAsync($"Merge it with Record A"); // the first of 2 potential matches
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
        await page.AssertBannerAsync("Success", $"TRN request for {StringHelper.JoinNonEmpty(' ', matchedPersonA.FirstName, matchedPersonA.MiddleName, matchedPersonA.LastName)} completed");

        await page.AssertBannerLinksToPersonRecord(matchedPersonA.PersonId);
    }

    [Fact]
    public async Task Resolve_NoLongerAnyPotentialMatches_CreateNewRecord()
    {
        await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest).ExecuteDeleteAsync());

        var firstName = TestData.GenerateFirstName();
        var middleName = TestData.GenerateMiddleName();
        var lastName = TestData.GenerateLastName();
        var dateOfBirth = TestData.GenerateDateOfBirth();
        var emailAddress = TestData.GenerateUniqueEmail();

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");

        var matchedPerson1 = await TestData.CreatePersonAsync(p =>
        {
            p.WithFirstName(firstName);
            p.WithMiddleName(middleName);
            p.WithLastName(lastName);
            p.WithDateOfBirth(dateOfBirth);
            p.WithEmailAddress(emailAddress);
        });

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t => t
                .WithMatchedPersons(matchedPerson1.PersonId)
                .WithStatus(SupportTaskStatus.Open)
                .WithFirstName(TestData.GenerateChangedFirstName(firstName))
                .WithMiddleName(TestData.GenerateChangedMiddleName(middleName))
                .WithLastName(TestData.GenerateChangedLastName(lastName))
                .WithDateOfBirth(TestData.GenerateChangedDateOfBirth(dateOfBirth))
                .WithEmailAddress(TestData.GenerateUniqueEmail())
            );

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
        await page.ClickButtonAsync("Create a record from this request");

        await page.AssertOnMatchesCheckYourAnswersPageAsync(supportTaskReference);
        await page.ClickButtonAsync("Confirm and create record");

        await page.AssertOnListPageAsync();
        await page.AssertBannerAsync("Success", $"TRN request for {StringHelper.JoinNonEmpty(' ', requestData.FirstName, requestData.MiddleName, requestData.LastName)} completed");

        await page.AssertBannerLinksToPersonRecord();
    }

    [Fact]
    public async Task Resolve_DefiniteMatchNowFound_MergeWithExistingRecord()
    {
        await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.Where(t => t.SupportTaskType == SupportTaskType.NpqTrnRequest).ExecuteDeleteAsync());

        var applicationUser = await TestData.CreateApplicationUserAsync(name: "NPQ");
        // Set up two records to merge with, where one has no conflicting info, one does have known conflicting info
        var matchedPerson1 = await TestData.CreatePersonAsync(p =>
        {
            p.WithEmailAddress(TestData.GenerateUniqueEmail());
            p.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var matchedPerson2 = await TestData.CreatePersonAsync(p =>
        {
            p.WithEmailAddress(TestData.GenerateUniqueEmail());
            p.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
        });
        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(
            applicationUser.UserId,
            t =>
            {
                t.WithMatchedPersons(matchedPerson1.PersonId, matchedPerson2.PersonId);
                t.WithNationalInsuranceNumber(matchedPerson1.NationalInsuranceNumber);
                t.WithDateOfBirth(matchedPerson1.DateOfBirth);
                t.WithEmailAddress(matchedPerson1.EmailAddress!);
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
        await page.ClickButtonAsync("Merge this request with Record A");

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
        await page.AssertBannerAsync("Success", $"TRN request for {StringHelper.JoinNonEmpty(' ', matchedPerson1.FirstName, matchedPerson1.MiddleName, matchedPerson1.LastName)} completed");

        await page.AssertBannerLinksToPersonRecord(matchedPerson1.PersonId);
    }

    [Fact]
    public async Task NoMatches_CreateRecord()
    {
        await WithDbContextAsync(dbContext =>
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
        await page.AssertBannerAsync("Success", $"TRN request for {StringHelper.JoinNonEmpty(' ', requestData.FirstName, requestData.MiddleName, requestData.LastName)} completed");

        await page.AssertBannerLinksToPersonRecord();
    }

    [Fact]
    public async Task RejectRequest()
    {
        await WithDbContextAsync(dbContext =>
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
