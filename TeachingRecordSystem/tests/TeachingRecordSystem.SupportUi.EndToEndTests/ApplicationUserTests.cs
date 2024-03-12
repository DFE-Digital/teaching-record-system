using System.Security.Cryptography;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class ApplicationUserTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task AddApplicationUser()
    {
        var applicationUserName = TestData.GenerateApplicationUserName();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToApplicationUsersPage();

        await page.AssertOnApplicationUsersPage();

        await page.ClickLinkForElementWithTestId("add-application-user");

        await page.AssertOnAddApplicationUserPage();

        await page.FillAsync("text=Name", applicationUserName);

        await page.ClickButton("Save");

        var applicationUserId = await WithDbContext(async dbContext =>
        {
            var applicationUser = await dbContext.ApplicationUsers.Where(u => u.Name == applicationUserName).SingleOrDefaultAsync();
            return applicationUser!.UserId;
        });

        await page.AssertOnEditApplicationUserPage(applicationUserId);

        await page.AssertFlashMessage("Application user added");
    }

    [Fact]
    public async Task EditApplicationUser()
    {
        var applicationUser = await TestData.CreateApplicationUser();
        var applicationUserId = applicationUser.UserId;
        var newApplicationUserName = TestData.GenerateChangedApplicationUserName(applicationUser.Name);
        var newClientId = Guid.NewGuid().ToString();
        var newClientSecret = Guid.NewGuid().ToString();
        var newRedirectUris = "https://localhost/callback";
        var newPostLogoutRedirectUris = "https://localhost/logout-callback";
        var newAuthenticationSchemeName = Guid.NewGuid().ToString();
        var newOneLoginClientId = Guid.NewGuid().ToString();
        var newOneLoginPrivateKeyPem = TestCommon.TestData.GeneratePrivateKeyPem();
        var newOneLoginRedirectUri = $"/_onelogin/{newAuthenticationSchemeName}/callback";
        var newOneLoginPostLogoutRedirectUri = $"/_onelogin/{newAuthenticationSchemeName}/logout-callback";

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToApplicationUsersPage();

        await page.AssertOnApplicationUsersPage();

        await page.ClickLinkForElementWithTestId($"edit-application-user-{applicationUserId}");

        await page.AssertOnEditApplicationUserPage(applicationUserId);

        await page.FillAsync("text=Name", newApplicationUserName);
        await page.SetCheckedAsync($"label:text-is('{ApiRoles.GetPerson}')", true);
        await page.SetCheckedAsync($"label:text-is('{ApiRoles.UpdatePerson}')", true);
        await page.SetCheckedAsync($"label:text-is('OIDC client')", true);
        await page.FillAsync("text=Client ID", newClientId);
        await page.FillAsync("text=Client secret", newClientSecret);
        await page.FillAsync("text=Redirect URIs", newRedirectUris);
        await page.FillAsync("text=Post logout redirect URIs", newPostLogoutRedirectUris);
        await page.FillAsync("text=Authentication scheme name", newAuthenticationSchemeName);
        await page.FillAsync("text=One Login client ID", newOneLoginClientId);
        await page.FillAsync("text=One Login private key", newOneLoginPrivateKeyPem);
        await page.FillAsync("text=One Login redirect URI path", newOneLoginRedirectUri);
        await page.FillAsync("text=One Login post logout redirect URI path", newOneLoginPostLogoutRedirectUri);

        await page.ClickButton("Save changes");

        await page.AssertOnApplicationUsersPage();

        await page.AssertFlashMessage("Application user updated");
    }

    [Fact]
    public async Task AddApiKey()
    {
        var applicationUser = await TestData.CreateApplicationUser();
        var applicationUserId = applicationUser.UserId;
        var apiKey = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToApplicationUsersPage();

        await page.AssertOnApplicationUsersPage();

        await page.ClickLinkForElementWithTestId($"edit-application-user-{applicationUserId}");

        await page.AssertOnEditApplicationUserPage(applicationUserId);

        await page.ClickLinkForElementWithTestId("AddApiKey");

        await page.AssertOnAddApiKeyPage();

        await page.FillAsync("label:text-is('Key')", apiKey);

        await page.ClickButton("Save");

        await page.AssertOnEditApplicationUserPage(applicationUserId);

        await page.AssertFlashMessage("API key added");
    }

    [Fact]
    public async Task EditApiKey()
    {
        var applicationUser = await TestData.CreateApplicationUser();
        var applicationUserId = applicationUser.UserId;
        var apiKey = await TestData.CreateApiKey(applicationUser.UserId);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToApplicationUsersPage();

        await page.AssertOnApplicationUsersPage();

        await page.ClickLinkForElementWithTestId($"edit-application-user-{applicationUserId}");

        await page.AssertOnEditApplicationUserPage(applicationUserId);

        await page.ClickLinkForElementWithTestId($"EditApiKey-{apiKey.ApiKeyId}");

        await page.AssertOnEditApiKeyPage(apiKey.ApiKeyId);

        await page.ClickButton("Expire");

        await page.AssertOnEditApplicationUserPage(applicationUserId);

        await page.AssertFlashMessage("API key expired");
    }
}
