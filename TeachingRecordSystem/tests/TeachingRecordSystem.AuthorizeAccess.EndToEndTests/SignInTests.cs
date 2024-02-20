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

        await page.GoToStartPage();

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

        await page.GoToStartPage();

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.FillAsync("text=What is your National Insurance number?", person.NationalInsuranceNumber!);
        await page.ClickButton("Continue");

        await page.AssertSignedIn(person.Trn!);
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUser_MatchesWithTrn()
    {
        var person = await TestData.CreatePerson(x => x.WithTrn().WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToStartPage();

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.FillAsync("text=What is your National Insurance number?", Faker.Identification.UkNationalInsuranceNumber());
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.FillAsync("label:text-is('Teacher reference number (TRN)')", person.Trn!);
        await page.ClickButton("Continue");

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

        await page.GoToStartPage();

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.FillAsync("text=What is your National Insurance number?", Faker.Identification.UkNationalInsuranceNumber());
        await page.ClickButton("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.FillAsync("label:text-is('Teacher reference number (TRN)')", await TestData.GenerateTrn());
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

        await page.GoToStartPage();

        await page.AssertSignedIn(person.Trn!);
    }

    [Fact]
    public async Task SignIn_KnownButUnverifiedUser()
    {
        var person = await TestData.CreatePerson(x => x.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person.PersonId);

        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.Email));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToStartPage();

        await page.AssertSignedIn(person.Trn!);
    }
}
