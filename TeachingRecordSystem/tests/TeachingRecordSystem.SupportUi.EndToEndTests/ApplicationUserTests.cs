using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core;

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
        var newOneLoginClientId = Guid.NewGuid().ToString();
        var newOneLoginPrivateKeyPem = TestCommon.TestData.GeneratePrivateKeyPem();

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToApplicationUsersPage();

        await page.AssertOnApplicationUsersPage();

        await page.ClickLinkForElementWithTestId($"edit-application-user-{applicationUserId}");

        await page.AssertOnEditApplicationUserPage(applicationUserId);

        await page.FillAsync("text=Name", newApplicationUserName);
        await page.SetCheckedAsync($"label:text-is('{ApiRoles.GetPerson}')", true);
        await page.SetCheckedAsync($"label:text-is('{ApiRoles.UpdatePerson}')", true);
        await page.FillAsync("text=Client ID", newOneLoginClientId);
        await page.FillAsync("text=Private Key PEM", newOneLoginPrivateKeyPem);

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
