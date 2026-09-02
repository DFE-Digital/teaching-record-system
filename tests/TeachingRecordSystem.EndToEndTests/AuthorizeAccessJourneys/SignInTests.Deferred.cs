using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.EndToEndTests.AuthorizeAccessJourneys;

public partial class SignInTests
{
    [Fact]
    public async Task SignIn_DeferredRecordMatchingPolicy_UserHasPendingIdVerificationTask()
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync(deferred: true);

        await page.WaitForUrlPathAsync("/pending-support-request");
    }

    [Fact]
    public async Task SignIn_DeferredRecordMatchingPolicy_UserHasPendingMatchingTask_ReturnsExistingTrnRequestIdAndSignsIn()
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        var applicationUserId = HostFixture.DeferredRecordMatchingPolicyApplicationUserId;
        var trnRequestId = Guid.NewGuid().ToString();

        await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithClientApplicationUserId(applicationUserId).WithTrnRequestId(trnRequestId));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync(deferred: true);

        await page.AssertSignedInWithDormantTrnRequestAsync(trnRequestId);

        await page.CloseAsync();
    }

    [Fact]
    public async Task SignIn_DeferredRecordMatchingPolicy_UserWasVerifiedViaSupportTaskButNotMatched_ReturnsExistingTrnRequestIdAndSignsIn()
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        var applicationUserId = HostFixture.DeferredRecordMatchingPolicyApplicationUserId;
        var trnRequestId = Guid.NewGuid().ToString();

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithClientApplicationUserId(applicationUserId).WithTrnRequestId(trnRequestId));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask);

            supportTask.Status = SupportTaskStatus.Closed;
            supportTask.Data = (OneLoginUserIdVerificationData)supportTask.Data with
            {
                Outcome = OneLoginUserIdVerificationOutcome.VerifiedOnlyWithoutMatches
            };

            await dbContext.SaveChangesAsync();
        });

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync(deferred: true);

        await page.AssertSignedInWithDormantTrnRequestAsync(trnRequestId);

        await page.CloseAsync();
    }

    [Fact]
    public async Task SignIn_DeferredRecordMatchingPolicy_VerifiedUserWithNoTrn_CreatesDormantTrnRequestAndSignsIn()
    {
        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(
            TestData.GenerateFirstName(),
            TestData.GenerateLastName(),
            TestData.GenerateDateOfBirth());
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync(deferred: true);

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickGovUkButtonAsync("Find your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", TestData.GenerateNationalInsuranceNumber());
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("label:text-is('No')");
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn-deferred");

        await page.ClickGovUkButtonAsync("Continue");

        var trnRequestId = await WithDbContextAsync(async dbContext =>
        {
            var trnRequest = await dbContext.TrnRequestMetadata
                .Where(r => r.OneLoginUserSubject == subject)
                .OrderByDescending(r => r.CreatedOn)
                .SingleOrDefaultAsync();

            Assert.NotNull(trnRequest);

            return trnRequest.RequestId;
        });

        await page.AssertSignedInWithDormantTrnRequestAsync(trnRequestId);

        await page.CloseAsync();
    }

    [Fact]
    public async Task SignIn_DeferredRecordMatchingPolicy_VerifiedUserWithUnmatchedTrn_CreatesSupportTaskWithDormantTrnRequestAndSignsIn()
    {
        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(
            TestData.GenerateFirstName(),
            TestData.GenerateLastName(),
            TestData.GenerateDateOfBirth());
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync(deferred: true);

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickGovUkButtonAsync("Find your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", TestData.GenerateNationalInsuranceNumber());
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('Teacher reference number')", "9999999");
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/not-found");
        await page.ClickGovUkButtonAsync("Next");

        var trainingProvider = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync())
            .Where(x => !x.Name.Contains('\''))
            .First();
        var qtsSubject = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync())
            .Where(x => !x.Name.Contains('\''))
            .First();

        await page.WaitForUrlPathAsync("/qts-status");
        await page.CheckAsync("text=Yes");
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/qts-details");
        await page.FillAsync("input#YearQtsReceived", TimeProvider.UtcNow.Year.ToString());
        await page.FillAutocompleteAsync("TrainingProviderId", trainingProvider.Name);
        await page.FillAutocompleteAsync("SubjectId", qtsSubject.Name);
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/check-answers");
        await page.ClickGovUkButtonAsync("Submit support request");

        await page.WaitForUrlPathAsync("/request-submitted");

        var (trnRequestId, supportTaskReference) = await WithDbContextAsync(async dbContext =>
        {
            var trnRequest = await dbContext.TrnRequestMetadata
                .Where(r => r.OneLoginUserSubject == subject)
                .OrderByDescending(r => r.CreatedOn)
                .SingleOrDefaultAsync();

            Assert.NotNull(trnRequest);
            Assert.Equal(TrnRequestStatus.Dormant, trnRequest.Status);

            var supportTask = await dbContext.SupportTasks
                .Where(st => st.OneLoginUserSubject == subject)
                .OrderByDescending(st => st.CreatedOn)
                .SingleOrDefaultAsync();

            Assert.NotNull(supportTask);
            Assert.Equal(SupportTaskType.OneLoginUserRecordMatching, supportTask.SupportTaskType);
            Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
            Assert.Equal(trnRequest.RequestId, supportTask.TrnRequestId);
            Assert.Equal(trnRequest.ApplicationUserId, supportTask.TrnRequestApplicationUserId);

            return (trnRequest.RequestId, supportTask.SupportTaskReference);
        });

        await page.ClickGovUkButtonAsync("Continue");

        await page.AssertSignedInWithDormantTrnRequestAsync(trnRequestId);

        await page.CloseAsync();
    }

    [Fact]
    public async Task SignIn_DeferredRecordMatchingPolicy_VerifiedUserWithUnmatchedTrn_ChoosesNoQts_GoesToCheckAnswers()
    {
        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(
            TestData.GenerateFirstName(),
            TestData.GenerateLastName(),
            TestData.GenerateDateOfBirth());
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync(deferred: true);

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickGovUkButtonAsync("Find your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", TestData.GenerateNationalInsuranceNumber());
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('Teacher reference number')", "9999999");
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/not-found");
        await page.ClickGovUkButtonAsync("Next");

        await page.WaitForUrlPathAsync("/qts-status");
        await page.CheckAsync("text=No");
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/check-answers");
    }

    [Fact]
    public async Task SignIn_DeferredRecordMatchingPolicy_VerifiedUserWithUnmatchedTrn_CanChangeQtsDetailsFromCheckAnswers()
    {
        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(
            TestData.GenerateFirstName(),
            TestData.GenerateLastName(),
            TestData.GenerateDateOfBirth());
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        var trainingProviders = (await TestData.ReferenceDataCache.GetTrainingProvidersAsync())
            .Where(x => !x.Name.Contains('\''))
            .Take(2)
            .ToArray();
        Assert.Equal(2, trainingProviders.Length);

        var subjects = (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync())
            .Where(x => !x.Name.Contains('\''))
            .Take(2)
            .ToArray();
        Assert.Equal(2, subjects.Length);

        var currentYear = TimeProvider.UtcNow.Year.ToString();
        var previousYear = (TimeProvider.UtcNow.Year - 1).ToString();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync(deferred: true);

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickGovUkButtonAsync("Find your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", TestData.GenerateNationalInsuranceNumber());
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('Teacher reference number')", "9999999");
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/not-found");
        await page.ClickGovUkButtonAsync("Next");

        await page.WaitForUrlPathAsync("/qts-status");
        await page.CheckAsync("text=Yes");
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/qts-details");
        await page.FillAsync("input#YearQtsReceived", currentYear);
        await page.FillAutocompleteAsync("TrainingProviderId", trainingProviders[0].Name);
        await page.FillAutocompleteAsync("SubjectId", subjects[0].Name);
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/check-answers");
        await page.AssertContentContainsAsync(currentYear, "Year received");
        await page.AssertContentContainsAsync(trainingProviders[0].Name, "Provider");
        await page.AssertContentContainsAsync(subjects[0].Name, "Subject");

        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Year received");
        await page.WaitForUrlPathAsync("/qts-details");
        await page.FillAsync("input#YearQtsReceived", previousYear);
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/check-answers");
        await page.AssertContentContainsAsync(previousYear, "Year received");
        await page.AssertContentContainsAsync(trainingProviders[0].Name, "Provider");
        await page.AssertContentContainsAsync(subjects[0].Name, "Subject");

        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Provider");
        await page.WaitForUrlPathAsync("/qts-details");
        await page.FillAutocompleteAsync("TrainingProviderId", trainingProviders[1].Name);
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/check-answers");
        await page.AssertContentContainsAsync(previousYear, "Year received");
        await page.AssertContentContainsAsync(trainingProviders[1].Name, "Provider");
        await page.AssertContentContainsAsync(subjects[0].Name, "Subject");

        await page.ClickChangeLinkForSummaryListRowWithKeyAsync("Subject");
        await page.WaitForUrlPathAsync("/qts-details");
        await page.FillAutocompleteAsync("SubjectId", subjects[1].Name);
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/check-answers");
        await page.AssertContentContainsAsync(previousYear, "Year received");
        await page.AssertContentContainsAsync(trainingProviders[1].Name, "Provider");
        await page.AssertContentContainsAsync(subjects[1].Name, "Subject");
    }
}
