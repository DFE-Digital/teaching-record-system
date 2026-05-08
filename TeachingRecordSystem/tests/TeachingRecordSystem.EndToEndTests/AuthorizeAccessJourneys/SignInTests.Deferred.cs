using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Playwright;
using TeachingRecordSystem.Core.ApiSchema;
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
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        var applicationUserId = await GetDeferredRecordMatchingPolicyApplicationUserId();
        var trnRequestId = Guid.NewGuid().ToString();

        await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithClientApplicationUserId(applicationUserId).WithTrnRequestId(trnRequestId));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync(deferred: true);

        await page.AssertSignedInWithDormantTrnRequestAsync(trnRequestId);

        await page.CloseAsync();

        await ActivateAndResolveDormantTrnRequestAndSignInAsync(context, trnRequestId);
    }

    [Fact]
    public async Task SignIn_DeferredRecordMatchingPolicy_UserWasVerifiedViaSupportTaskButNotMatched_ReturnsExistingTrnRequestIdAndSignsIn()
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        var applicationUserId = await GetDeferredRecordMatchingPolicyApplicationUserId();
        var trnRequestId = Guid.NewGuid().ToString();

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithClientApplicationUserId(applicationUserId).WithTrnRequestId(trnRequestId));

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(supportTask);

            supportTask.Status = SupportTaskStatus.Closed;
            supportTask.UpdateData<OneLoginUserIdVerificationData>(
                data => data with { Outcome = OneLoginUserIdVerificationOutcome.VerifiedOnlyWithoutMatches });

            await dbContext.SaveChangesAsync();
        });

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync(deferred: true);

        await page.AssertSignedInWithDormantTrnRequestAsync(trnRequestId);

        await page.CloseAsync();

        await ActivateAndResolveDormantTrnRequestAndSignInAsync(context, trnRequestId);
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

        await ActivateAndResolveDormantTrnRequestAndSignInAsync(context, trnRequestId);
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
        await page.ClickGovUkButtonAsync("Check your answers");

        await page.WaitForUrlPathAsync("/check-answers");
        await page.ClickGovUkButtonAsync("Submit support request");

        await page.WaitForUrlPathAsync("/request-submitted");

        var trnRequestId = await WithDbContextAsync(async dbContext =>
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

            return trnRequest.RequestId;
        });

        await page.GetByTestId("continue-link").ClickAsync();

        await page.AssertSignedInWithDormantTrnRequestAsync(trnRequestId);

        await page.CloseAsync();

        await ActivateAndResolveDormantTrnRequestAndSignInAsync(context, trnRequestId);
    }

    private async Task ActivateAndResolveDormantTrnRequestAndSignInAsync(IBrowserContext context, string trnRequestId)
    {
        var resolvedTrn = await ResolveDormantTrnRequestAsync();

        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync(deferred: true);

        await page.AssertSignedInAsync(resolvedTrn);

        async Task<string> ResolveDormantTrnRequestAsync()
        {
            var applicationUserId = await GetDeferredRecordMatchingPolicyApplicationUserId();

            using var httpClient = HostFixture.GetHttpClientWithAuthorizeAccessTokenForTrnRequest(
                applicationUserId,
                trnRequestId,
                version: VersionRegistry.V3MinorVersions.V20260416);

            var activateResponse = await httpClient.PutAsync("/v3/trn-request/activate", content: null);
            activateResponse.EnsureSuccessStatusCode();

            var activateResponseBodyJson = (await activateResponse.Content.ReadFromJsonAsync<JsonDocument>())!;

            var status = activateResponseBodyJson.RootElement.GetProperty("status").GetString();
            if (status is not "Completed")
            {
                throw new InvalidOperationException($"Unexpected status '{status}' when activating dormant TRN request.");
            }

            var trn = activateResponseBodyJson.RootElement.GetProperty("trn").GetString()!;
            return trn;
        }
    }

    private Task<Guid> GetDeferredRecordMatchingPolicyApplicationUserId() =>
        WithDbContextAsync(dbContext => dbContext.ApplicationUsers
            .Where(u => u.OneLoginAuthenticationSchemeName == HostFixture.DeferredFakeOneLoginAuthenticationScheme)
            .Select(u => u.UserId)
            .SingleAsync());
}
