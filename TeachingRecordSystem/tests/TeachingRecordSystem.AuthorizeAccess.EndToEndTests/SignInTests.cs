using Microsoft.Playwright;
using Optional;
using static TeachingRecordSystem.AuthorizeAccess.IdModelTypes;

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

        await page.GoToTestStartPageAsync();

        await page.WaitForUrlPathAsync("/not-verified");
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUser_MatchesWithNino()
    {
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber());

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickButtonAsync("Find your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", person.NationalInsuranceNumber!);
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/found");
        await page.ClickButtonAsync("Access your teaching record");

        await page.AssertSignedInAsync(person.Trn);
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUserWithUnmatchedNino_MatchesWithTrn()
    {
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickButtonAsync("Find your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", TestData.GenerateNationalInsuranceNumber());
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('Teacher reference number')", person.Trn);
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/found");
        await page.ClickButtonAsync("Access your teaching record");

        await page.AssertSignedInAsync(person.Trn);
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUserWithoutNino_MatchesWithTrn()
    {
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickButtonAsync("Find your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=No");
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('Teacher reference number')", person.Trn);
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/found");
        await page.ClickButtonAsync("Access your teaching record");

        await page.AssertSignedInAsync(person.Trn);
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUser_DoesNotMatchWithNinoOrTrn()
    {
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickButtonAsync("Find your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", TestData.GenerateNationalInsuranceNumber());
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('Teacher reference number')", await TestData.GenerateTrnAsync());
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/not-found");
        await page.ClickButtonAsync("Check your answers");

        await page.WaitForUrlPathAsync("/check-answers");
        await page.ClickButtonAsync("Submit support request");

        await page.WaitForUrlPathAsync("/request-submitted");
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUserWithNeitherNinoNorTrn_DoesNotMatch()
    {
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.WaitForUrlPathAsync("/connect");
        await page.ClickButtonAsync("Find your teaching record");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=No");
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("label:text-is('No')");
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/no-trn");
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUserWithTrnTokenAndMatchingDetails_MatchesWithTrn()
    {
        var person = await TestData.CreatePersonAsync(x => x.WithNationalInsuranceNumber(false));

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        var trnToken = Guid.NewGuid().ToString();

        using (var idDbContext = HostFixture.Services.GetRequiredService<IdDbContext>())
        {
            idDbContext.TrnTokens.Add(new IdTrnToken()
            {
                TrnToken = trnToken,
                Trn = person.Trn,
                CreatedUtc = Clock.UtcNow,
                ExpiresUtc = Clock.UtcNow.AddDays(1),
                Email = email,
                UserId = null
            });

            await idDbContext.SaveChangesAsync();
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync(trnToken: trnToken);

        await page.AssertSignedInAsync(person.Trn);
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUserWithGetAnIdentityAccountAndMatchingDetails_MatchesWithTrn()
    {
        var person = await TestData.CreatePersonAsync();

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        using (var scope = HostFixture.Services.CreateScope())
        {
            using var idDbContext = scope.ServiceProvider.GetRequiredService<IdDbContext>();

            idDbContext.Users.Add(new User()
            {
                UserId = Guid.NewGuid(),
                EmailAddress = email,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Created = Clock.UtcNow,
                Updated = Clock.UtcNow,
                UserType = IdModelTypes.UserType.Teacher,
                TrnVerificationLevel = TrnVerificationLevel.Medium,
                Trn = person.Trn
            });

            await idDbContext.SaveChangesAsync();
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.AssertSignedInAsync(person.Trn);
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUserWithGetAnIdentityAccountWithTrnAssociatedByTrnTokenAndMatchingDetails_MatchesWithTrn()
    {
        var person = await TestData.CreatePersonAsync();

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        using (var scope = HostFixture.Services.CreateScope())
        {
            using var idDbContext = scope.ServiceProvider.GetRequiredService<IdDbContext>();

            idDbContext.Users.Add(new User()
            {
                UserId = Guid.NewGuid(),
                EmailAddress = email,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Created = Clock.UtcNow,
                Updated = Clock.UtcNow,
                UserType = IdModelTypes.UserType.Teacher,
                TrnVerificationLevel = TrnVerificationLevel.Low,
                TrnAssociationSource = TrnAssociationSource.TrnToken,
                Trn = person.Trn
            });

            await idDbContext.SaveChangesAsync();
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.AssertSignedInAsync(person.Trn);
    }

    [Fact]
    public async Task SignIn_UnknownVerifiedUserWithGetAnIdentityAccountWithTrnAssociatedBySupportAndMatchingDetails_MatchesWithTrn()
    {
        var person = await TestData.CreatePersonAsync();

        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email, coreIdentityVc));

        using (var scope = HostFixture.Services.CreateScope())
        {
            using var idDbContext = scope.ServiceProvider.GetRequiredService<IdDbContext>();

            idDbContext.Users.Add(new User()
            {
                UserId = Guid.NewGuid(),
                EmailAddress = email,
                FirstName = person.FirstName,
                LastName = person.LastName,
                Created = Clock.UtcNow,
                Updated = Clock.UtcNow,
                UserType = IdModelTypes.UserType.Teacher,
                TrnVerificationLevel = TrnVerificationLevel.Low,
                TrnAssociationSource = TrnAssociationSource.SupportUi,
                Trn = person.Trn
            });

            await idDbContext.SaveChangesAsync();
        }

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.AssertSignedInAsync(person.Trn);
    }

    [Fact]
    public async Task SignIn_KnownUser()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.AssertSignedInAsync(person.Trn);
    }

    [Fact]
    public async Task SignIn_UnknownUser_OneLoginUserIsAttachedToTrnRequest()
    {
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();

        var subject = TestData.CreateOneLoginUserSubject();

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrnRequest(applicationUser.UserId, trnRequestId, identityVerified: true, oneLoginUserSubject: subject));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(subject: Option.Some(subject));

        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.AssertSignedInAsync(person.Trn);
    }

    [Fact]
    public async Task SignIn_UnknownUser_EmailIsAttachedToTrnRequest()
    {
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var trnRequestId = Guid.NewGuid().ToString();

        var email = TestData.GenerateUniqueEmail();

        var person = await TestData.CreatePersonAsync(p => p
            .WithEmailAddress(email)
            .WithTrnRequest(applicationUser.UserId, trnRequestId, identityVerified: true));

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(email: Option.Some((string?)email));

        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.AssertSignedInAsync(person.Trn);
    }

    [Fact]
    public async Task SignIn_UserHasPendingTask()
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync();
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!));

        await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.WaitForUrlPathAsync("/pending-support-request");
    }

    [Fact]
    public async Task SignIn_UnknownUnverifiedUser()
    {
        var subject = TestData.CreateOneLoginUserSubject();
        var email = Faker.Internet.Email();
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(subject, email));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToTestStartPageAsync();

        await page.WaitForUrlPathAsync("/not-verified");
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/name");
        await page.FillAsync("label:text-is('First name')", TestData.GenerateFirstName());
        await page.FillAsync("label:text-is('Last name')", TestData.GenerateLastName());
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/date-of-birth");
        await page.FillAsync("label:text-is('Day')", "15");
        await page.FillAsync("label:text-is('Month')", "06");
        await page.FillAsync("label:text-is('Year')", "1990");
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/national-insurance-number");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('National Insurance number')", TestData.GenerateNationalInsuranceNumber());
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/trn");
        await page.CheckAsync("text=Yes");
        await page.FillAsync("label:text-is('Teacher reference number')", await TestData.GenerateTrnAsync());
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/proof-of-identity");
        await page
            .Locator("input[type='file']")
            .SetInputFilesAsync(
                new FilePayload
                {
                    Name = "identity.jpg",
                    MimeType = "image/jpeg",
                    Buffer = TestData.JpegImage
                });
        await page.ClickButtonAsync("Continue");

        await page.WaitForUrlPathAsync("/check-answers");
        await page.ClickButtonAsync("Submit support request");

        await page.WaitForUrlPathAsync("/request-submitted");
    }
}
