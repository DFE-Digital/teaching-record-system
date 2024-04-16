using System.Text.Json;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public class OidcTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task SignInAndOut()
    {
        var person = await TestData.CreatePerson(x => x.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person);

        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.Email, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/oidc-test");
        await page.ClickButton("Start");
        await page.WaitForUrlPathAsync("/oidc-test/signed-in");

        string[][] expectedVerifiedNames = [[person.FirstName, person.LastName]];
        DateOnly[] expectedVerifiedBirthDates = [person.DateOfBirth];

        var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(await page.GetByLabel("Claims").InputValueAsync()) ?? [];
        Assert.Equal(oneLoginUser.Subject, claims.GetValueOrDefault("sub"));
        Assert.Equal(person.Trn, claims.GetValueOrDefault("trn"));
        Assert.Equal(oneLoginUser.Email, claims.GetValueOrDefault("email"));
        Assert.NotEmpty(claims.GetValueOrDefault("onelogin_id") ?? "");
        Assert.Equal(expectedVerifiedNames, JsonSerializer.Deserialize<string[][]>(claims.GetValueOrDefault("onelogin_verified_names") ?? "[]"));
        Assert.Equal(expectedVerifiedBirthDates, JsonSerializer.Deserialize<DateOnly[]>(claims.GetValueOrDefault("onelogin_verified_birthdates") ?? "[]"));

        await page.ClickAsync("a:text-is('Sign out')");
        await page.WaitForUrlPathAsync("/oidc-test");
    }
}
