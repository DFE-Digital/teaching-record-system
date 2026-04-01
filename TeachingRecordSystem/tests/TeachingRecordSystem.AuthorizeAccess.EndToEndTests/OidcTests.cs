using System.Text.Json;

namespace TeachingRecordSystem.AuthorizeAccess.EndToEndTests;

public class OidcTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task SignInAndOut_ReturnsClaimsAndTokenAsExpected()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);
        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!, coreIdentityVc));

        string[] expectedVerifiedName = [person.FirstName, person.LastName];
        DateOnly expectedVerifiedDateOfBirth = person.DateOfBirth;

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        // Act
        await page.GotoAsync("/oidc-test");
        await page.ClickButtonAsync("Start");
        await page.WaitForUrlPathAsync("/oidc-test/signed-in");

        // Assert
        var accessToken = await page.GetByLabel("Access token").InputValueAsync();
        Assert.NotNull(accessToken);
        Assert.NotEmpty(accessToken);

        var refreshToken = await page.GetByLabel("Refresh token").InputValueAsync();
        Assert.NotNull(refreshToken);
        Assert.NotEmpty(refreshToken);

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

        var coreIdentityVc = TestData.CreateOneLoginCoreIdentityVc(person.FirstName, person.LastName, person.DateOfBirth);
        SetCurrentOneLoginUser(OneLoginUserInfo.Create(oneLoginUser.Subject, oneLoginUser.EmailAddress!, coreIdentityVc));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GotoAsync("/oidc-test");
        await page.ClickGovUkButtonAsync("Start");
        await page.WaitForUrlPathAsync("/oidc-test/signed-in");

        var initialAccessToken = await page.GetByLabel("Access token").InputValueAsync();
        var initialRefreshToken = await page.GetByLabel("Refresh token").InputValueAsync();

        Assert.NotNull(initialAccessToken);
        Assert.NotEmpty(initialAccessToken);
        Assert.NotNull(initialRefreshToken);
        Assert.NotEmpty(initialRefreshToken);

        await page.ClickAsync("a:text-is('Test refresh token')");
        await page.WaitForUrlPathAsync("/oidc-test/refresh-token");

        await page.ClickButtonAsync("Exchange refresh token for new access token");
        await page.WaitForSelectorAsync("input#OldAccessToken[value]:not([value=''])", new() { Timeout = 10000 });

        var oldAccessToken = await page.GetByLabel("Old access token").InputValueAsync();
        var oldRefreshToken = await page.GetByLabel("Old refresh token").InputValueAsync();
        Assert.Equal(initialAccessToken, oldAccessToken);
        Assert.Equal(initialRefreshToken, oldRefreshToken);

        var currentAccessToken = await page.GetByLabel("Current access token").InputValueAsync();
        var currentRefreshToken = await page.GetByLabel("Current refresh token").InputValueAsync();

        Assert.NotNull(currentAccessToken);
        Assert.NotEmpty(currentAccessToken);
        Assert.NotEqual(initialAccessToken, currentAccessToken);
        Assert.NotNull(currentRefreshToken);
        Assert.NotEmpty(currentRefreshToken);
        Assert.NotEqual(initialRefreshToken, currentRefreshToken);
    }
}
