namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public class UserTests : TestBase
{
    public UserTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task AddUser()
    {
        var testAzAdUser = TestUsers.TestAzureActiveDirectoryUser;
        await TestData.CreateCrmUser(azureAdUserId: Guid.Parse(testAzAdUser.UserId), dqtRoles: ["CRM Helpdesk"]);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPage();

        await page.AssertOnUsersPage();

        await page.ClickLinkForElementWithTestId("add-user");

        await page.AssertOnAddUserPage();

        await page.FillEmailInput(testAzAdUser.Email);

        await page.ClickButton("Find user");

        await page.AssertOnAddUserConfirmPage();

        await page.SetCheckedAsync("label:text-is('Helpdesk')", true);

        await page.ClickButton("Add user");

        await page.AssertOnUsersPage();

        await page.AssertFlashMessage("User added");
    }

    [Fact]
    public async Task EditUser()
    {
        var azAdUserId = Guid.NewGuid();
        await TestData.CreateCrmUser(azureAdUserId: azAdUserId, dqtRoles: ["CRM Helpdesk"]);
        var user = await TestData.CreateUser(azureAdUserId: azAdUserId, roles: ["Administrator"]);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPage();

        await page.AssertOnUsersPage();

        await page.ClickLinkForElementWithTestId($"edit-user-{user.UserId}");

        await page.AssertOnEditUserPage(user.UserId);

        await page.SetCheckedAsync("label:text-is('Administrator')", false);
        await page.SetCheckedAsync("label:text-is('Helpdesk')", true);

        await page.ClickButton("Save changes");

        await page.AssertOnUsersPage();

        await page.AssertFlashMessage("User updated");
    }

    [Fact]
    public async Task DeactivateUser()
    {
        var azAdUserId = Guid.NewGuid();
        await TestData.CreateCrmUser(azureAdUserId: azAdUserId, dqtRoles: ["CRM Helpdesk"]);
        var user = await TestData.CreateUser(azureAdUserId: azAdUserId, roles: ["Administrator"]);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPage();

        await page.AssertOnUsersPage();

        await page.ClickLinkForElementWithTestId($"edit-user-{user.UserId}");

        await page.AssertOnEditUserPage(user.UserId);

        await page.ClickButton("Deactivate");

        await page.AssertOnUsersPage();

        await page.AssertFlashMessage("User deactivated");
    }

    [Fact]
    public async Task ReactivateUser()
    {
        var azAdUserId = Guid.NewGuid();
        await TestData.CreateCrmUser(azureAdUserId: azAdUserId, dqtRoles: ["CRM Helpdesk"]);
        var user = await TestData.CreateUser(active: false, azureAdUserId: azAdUserId, roles: ["Administrator"]);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPage();

        await page.AssertOnUsersPage();

        await page.ClickLinkForElementWithTestId($"edit-user-{user.UserId}");

        await page.AssertOnEditUserPage(user.UserId);

        await page.ClickButton("Reactivate");

        await page.AssertOnEditUserPage(user.UserId);

        await page.AssertFlashMessage("User reactivated");
    }
}
