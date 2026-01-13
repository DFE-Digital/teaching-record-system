using System.Security.Cryptography;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Users;

public class ApplicationUserTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task AddApplicationUser()
    {
        var applicationUserName = TestData.GenerateApplicationUserName();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToApplicationUsersPageAsync();

        await page.AssertOnApplicationUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync("add-application-user");

        await page.AssertOnAddApplicationUserPageAsync();

        await page.FillAsync("text=Name", applicationUserName);

        await page.ClickButtonAsync("Save");

        var applicationUserId = await WithDbContextAsync(async dbContext =>
        {
            var applicationUser = await dbContext.ApplicationUsers.Where(u => u.Name == applicationUserName).SingleOrDefaultAsync();
            return applicationUser!.UserId;
        });

        await page.AssertOnEditApplicationUserPageAsync(applicationUserId);

        await page.AssertFlashMessageAsync("Application user added");
    }

    [Fact]
    public async Task EditApplicationUser()
    {
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var applicationUserId = applicationUser.UserId;
        var newApplicationUserName = TestData.GenerateChangedApplicationUserName(applicationUser.Name);
        var newClientId = Guid.NewGuid().ToString();
        var newClientSecret = Guid.NewGuid().ToString();
        var newRedirectUris = "https://localhost/callback";
        var newPostLogoutRedirectUris = "https://localhost/logout-callback";
        var newAuthenticationSchemeName = Guid.NewGuid().ToString();
        var newOneLoginClientId = Guid.NewGuid().ToString();
        var newOneLoginPrivateKeyPem = TestData.GeneratePrivateKeyPem();
        var newOneLoginRedirectUri = $"/_onelogin/{newAuthenticationSchemeName}/callback";
        var newOneLoginPostLogoutRedirectUri = $"/_onelogin/{newAuthenticationSchemeName}/logout-callback";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToApplicationUsersPageAsync();

        await page.AssertOnApplicationUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-application-user-{applicationUserId}");

        await page.AssertOnEditApplicationUserPageAsync(applicationUserId);

        await page.FillAsync("text=Name", newApplicationUserName);
        await page.SetCheckedAsync($"label{TextIsSelector(ApiRoles.GetPerson)}", true);
        await page.SetCheckedAsync($"label{TextIsSelector(ApiRoles.UpdatePerson)}", true);
        await page.SetCheckedAsync($"label:text-is('OIDC client')", true);
        await page.FillAsync("text=Client ID", newClientId);
        await page.FillAsync("text=Client secret", newClientSecret);
        await page.FillAsync("text=Redirect URIs", newRedirectUris);
        await page.FillAsync("text=Post logout redirect URIs", newPostLogoutRedirectUris);
        await page.FillAsync("text=Authentication scheme name", newAuthenticationSchemeName);
        await page.FillAsync("text=One Login client ID", newOneLoginClientId);
        await page.CheckAsync("input[value='False']:below(legend:has-text('Use shared One Login signing keys'))");
        await page.FillAsync("text=One Login private key", newOneLoginPrivateKeyPem);
        await page.FillAsync("text=One Login redirect URI path", newOneLoginRedirectUri);
        await page.FillAsync("text=One Login post logout redirect URI path", newOneLoginPostLogoutRedirectUri);

        await page.ClickButtonAsync("Save changes");

        await page.AssertOnApplicationUsersPageAsync();

        await page.AssertFlashMessageAsync("Application user updated");
    }

    [Fact]
    public async Task AddApiKey()
    {
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var applicationUserId = applicationUser.UserId;
        var apiKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToApplicationUsersPageAsync();

        await page.AssertOnApplicationUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-application-user-{applicationUserId}");

        await page.AssertOnEditApplicationUserPageAsync(applicationUserId);

        await page.ClickLinkForElementWithTestIdAsync("AddApiKey");

        await page.AssertOnAddApiKeyPageAsync();

        await page.FillAsync("label:text-is('Key')", apiKey);

        await page.ClickButtonAsync("Save");

        await page.AssertOnEditApplicationUserPageAsync(applicationUserId);

        await page.AssertFlashMessageAsync("API key added");
    }

    [Fact]
    public async Task EditApiKey()
    {
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var applicationUserId = applicationUser.UserId;
        var apiKey = await TestData.CreateApiKeyAsync(applicationUser.UserId);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToApplicationUsersPageAsync();

        await page.AssertOnApplicationUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-application-user-{applicationUserId}");

        await page.AssertOnEditApplicationUserPageAsync(applicationUserId);

        await page.ClickLinkForElementWithTestIdAsync($"EditApiKey-{apiKey.ApiKeyId}");

        await page.AssertOnEditApiKeyPageAsync(apiKey.ApiKeyId);

        await page.ClickButtonAsync("Expire");

        await page.AssertOnEditApplicationUserPageAsync(applicationUserId);

        await page.AssertFlashMessageAsync("API key expired");
    }
}
