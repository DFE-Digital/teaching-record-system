namespace TeachingRecordSystem.EndToEndTests.AuthorizeAccessJourneys;

public partial class SignInTests
{
    [Fact]
    public async Task SignIn_RequiredRecordMatchingPolicy_UserHasPendingIdVerificationTask()
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync();

        await page.WaitForUrlPathAsync("/pending-support-request");
    }

    [Fact]
    public async Task SignIn_RequiredRecordMatchingPolicy_UserHasPendingMatchingTask()
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync();

        await page.WaitForUrlPathAsync("/pending-support-request");
    }

    [Fact]
    public async Task SignIn_RequiredRecordMatchingPolicy_UnknownVerifiedUserWithNeitherNinoNorTrn_DoesNotMatch()
    {
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToAuthorizeAccessTestStartPageAsync();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickGovUkButtonAsync("Find your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=No");
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("label:text-is('No')");
        await page.ClickGovUkButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/no-trn");
    }
}
