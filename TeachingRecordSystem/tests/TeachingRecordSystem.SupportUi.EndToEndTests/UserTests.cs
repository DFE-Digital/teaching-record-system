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
        await TestData.CreateCrmUserAsync(azureAdUserId: Guid.Parse(testAzAdUser.UserId), dqtRoles: ["CRM Helpdesk"]);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPageAsync();

        await page.AssertOnUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync("add-user");

        await page.AssertOnAddUserPageAsync();

        await page.FillEmailInputAsync(testAzAdUser.Email);

        await page.ClickButtonAsync("Find user");

        await page.AssertOnAddUserConfirmPageAsync();

        await page.SetCheckedAsync("label:text-is('Helpdesk')", true);

        await page.ClickButtonAsync("Save");

        await page.AssertOnUsersPageAsync();

        await page.AssertFlashMessageAsync("User added");
    }

    [Fact]
    public async Task EditUser()
    {
        var azAdUserId = Guid.NewGuid();
        await TestData.CreateCrmUserAsync(azureAdUserId: azAdUserId, dqtRoles: ["CRM Helpdesk"]);
        var user = await TestData.CreateUserAsync(azureAdUserId: azAdUserId, roles: ["Administrator"]);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPageAsync();

        await page.AssertOnUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-user-{user.UserId}");

        await page.AssertOnEditUserPageAsync(user.UserId);

        await page.SetCheckedAsync("label:text-is('Administrator')", false);
        await page.SetCheckedAsync("label:text-is('Helpdesk')", true);

        await page.ClickButtonAsync("Save changes");

        await page.AssertOnUsersPageAsync();

        await page.AssertFlashMessageAsync("User updated");
    }

    [Fact]
    public async Task DeactivateUser()
    {
        var azAdUserId = Guid.NewGuid();
        await TestData.CreateCrmUserAsync(azureAdUserId: azAdUserId, dqtRoles: ["CRM Helpdesk"]);
        var user = await TestData.CreateUserAsync(azureAdUserId: azAdUserId, roles: ["Administrator"]);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPageAsync();

        await page.AssertOnUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-user-{user.UserId}");

        await page.AssertOnEditUserPageAsync(user.UserId);

        await page.ClickButtonAsync("Deactivate");

        await page.AssertOnUsersPageAsync();

        await page.AssertFlashMessageAsync("User deactivated");
    }

    [Fact]
    public async Task ReactivateUser()
    {
        var azAdUserId = Guid.NewGuid();
        await TestData.CreateCrmUserAsync(azureAdUserId: azAdUserId, dqtRoles: ["CRM Helpdesk"]);
        var user = await TestData.CreateUserAsync(active: false, azureAdUserId: azAdUserId, roles: ["Administrator"]);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPageAsync();

        await page.AssertOnUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-user-{user.UserId}");

        await page.AssertOnEditUserPageAsync(user.UserId);

        await page.ClickButtonAsync("Reactivate");

        await page.AssertOnEditUserPageAsync(user.UserId);

        await page.AssertFlashMessageAsync("User reactivated");
    }
}
