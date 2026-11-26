using System.Text.Json;

namespace TeachingRecordSystem.EndToEndTests.AuthorizeAccessJourneys;

public class OidcTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task SignInAndOut_ReturnsClaimsAndTokenAsExpected()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth!.Value);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!, coreIdentityVc));

        string[] expectedVerifiedName = [person.FirstName, person.LastName];
        DateOnly expectedVerifiedDateOfBirth = person.DateOfBirth!.Value;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        // Act
        await page.GotoAsync($"{AuthorizeAccessBaseUrl}/oidc-test");
        await page.ClickGovUkStartButtonAsync();
        await page.WaitForUrlPathAsync("/oidc-test/signed-in");

        // Assert
        await Expect(page.GetByLabel("Access token")).Not.ToBeEmptyAsync();
        await Expect(page.GetByLabel("Refresh token")).Not.ToBeEmptyAsync();

        var claims = JsonSerializer.Deserialize<Dictionary<string, string>>(await page.GetByLabel("Claims").InputValueAsync()) ?? [];
        Assert.Equal(oneLoginUser.Subject, claims.GetValueOrDefault("sub"));
        Assert.Equal(person.Trn, claims.GetValueOrDefault("trn"));
        Assert.Equal(oneLoginUser.EmailAddress, claims.GetValueOrDefault("email"));
        Assert.NotEmpty(claims.GetValueOrDefault("_ta_olidt") ?? "");
        Assert.Equal(expectedVerifiedName, JsonSerializer.Deserialize<string[]>(claims.GetValueOrDefault("verified_name") ?? "[]"));
        Assert.Equal(expectedVerifiedDateOfBirth, DateOnly.Parse(claims.GetValueOrDefault("verified_date_of_birth") ?? "0001-01-01"));

        await page.ClickAsync("text=Sign out");
        await page.WaitForUrlPathAsync("/oidc-test");
    }

    [Fact]
    public async Task RefreshToken_ExchangeForNewAccessToken_ReturnsNewTokens()
    {
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth!.Value);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync($"{AuthorizeAccessBaseUrl}/oidc-test");
        await page.ClickGovUkStartButtonAsync();
        await page.WaitForUrlPathAsync("/oidc-test/signed-in");

        await Expect(page.GetByLabel("Access token")).Not.ToBeEmptyAsync();
        await Expect(page.GetByLabel("Refresh token")).Not.ToBeEmptyAsync();

        var initialAccessToken = await page.GetByLabel("Access token").InputValueAsync();
        var initialRefreshToken = await page.GetByLabel("Refresh token").InputValueAsync();

        await page.ClickAsync("a:text-is('Test refresh token')");
        await page.WaitForUrlPathAsync("/oidc-test/refresh-token");

        await page.ClickButtonAsync("Exchange refresh token for new access token");

        await Expect(page.GetByLabel("Old access token")).ToHaveValueAsync(initialAccessToken, new() { Timeout = 10000 });
        await Expect(page.GetByLabel("Old refresh token")).ToHaveValueAsync(initialRefreshToken);

        await Expect(page.GetByLabel("Current access token")).Not.ToBeEmptyAsync();
        await Expect(page.GetByLabel("Current access token")).Not.ToHaveValueAsync(initialAccessToken);
        await Expect(page.GetByLabel("Current refresh token")).Not.ToBeEmptyAsync();
        await Expect(page.GetByLabel("Current refresh token")).Not.ToHaveValueAsync(initialRefreshToken);
    }
}
