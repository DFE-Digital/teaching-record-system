namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public class SignInTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task SignIn_UnknownUserNotVerified()
    {
        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPage();

        await page.WaitForUrlPathAsync("/not-verified");
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUser_MatchesWithNino()
    {
        var person = await TestData.CreatePerson(x => x.WithTrn().WithNationalInsuranceNumber());

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPage();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickButton("Connect to your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", person.NationalInsuranceNumber!);
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/found");
        await page.ClickButton("Access your teaching record");

        await page.AssertSignedIn(person.Trn!);
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUserWithUnmatchedNino_MatchesWithTrn()
    {
        var person = await TestData.CreatePerson(x => x.WithTrn().WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPage();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickButton("Connect to your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", TestData.GenerateNationalInsuranceNumber());
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('TRN')", person.Trn!);
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/found");
        await page.ClickButton("Access your teaching record");

        await page.AssertSignedIn(person.Trn!);
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUserWithoutNino_MatchesWithTrn()
    {
        var person = await TestData.CreatePerson(x => x.WithTrn().WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPage();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickButton("Connect to your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=No");
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('TRN')", person.Trn!);
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/found");
        await page.ClickButton("Access your teaching record");

        await page.AssertSignedIn(person.Trn!);
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUser_DoesNotMatchWithNinoOrTrn()
    {
        var person = await TestData.CreatePerson(x => x.WithTrn().WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPage();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickButton("Connect to your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", TestData.GenerateNationalInsuranceNumber());
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('TRN')", await TestData.GenerateTrn());
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/check-answers");
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/not-found");
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUserWithNeitherNinoNorTrn_DoesNotMatch()
    {
        var person = await TestData.CreatePerson(x => x.WithTrn().WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPage();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickButton("Connect to your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=No");
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=No");
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/check-answers");
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/not-found");
    }

    [Fact]
    public async Task SignIn_KnownUser()
    {
        var person = await TestData.CreatePerson(x => x.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person.PersonId);

        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.Email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPage();

        await page.AssertSignedIn(person.Trn!);
    }
}
