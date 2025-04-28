namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

public class LegacyUserTests : TestBase
{
    public LegacyUserTests(HostFixture hostFixture)
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

        await page.GoToLegacyUsersPageAsync();

        await page.AssertOnLegacyUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync("add-user");

        await page.AssertOnAddLegacyUserPageAsync();

        await page.FillEmailInputAsync(testAzAdUser.Email);

        await page.ClickButtonAsync("Find user");

        await page.AssertOnLegacyAddUserConfirmPageAsync();

        await page.SetCheckedAsync("label:text-is('Helpdesk')", true);

        await page.ClickButtonAsync("Save");

        await page.AssertOnLegacyUsersPageAsync();

        await page.AssertFlashMessageAsync("User added");
    }

    [Fact]
    public async Task EditUser()
    {
        var azAdUserId = Guid.NewGuid();
        await TestData.CreateCrmUserAsync(azureAdUserId: azAdUserId, dqtRoles: ["CRM Helpdesk"]);
        var user = await TestData.CreateUserAsync(azureAdUserId: azAdUserId, role: UserRoles.Administrator);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToLegacyUsersPageAsync();

        await page.AssertOnLegacyUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-user-{user.UserId}");

        await page.AssertOnLegacyEditUserPageAsync(user.UserId);

        await page.SetCheckedAsync("label:text-is('Administrator')", false);
        await page.SetCheckedAsync("label:text-is('Helpdesk')", true);

        await page.ClickButtonAsync("Save changes");

        await page.AssertOnLegacyUsersPageAsync();

        await page.AssertFlashMessageAsync("User updated");
    }

    [Fact]
    public async Task DeactivateUser()
    {
        var azAdUserId = Guid.NewGuid();
        await TestData.CreateCrmUserAsync(azureAdUserId: azAdUserId, dqtRoles: ["CRM Helpdesk"]);
        var user = await TestData.CreateUserAsync(azureAdUserId: azAdUserId, role: UserRoles.Administrator);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToLegacyUsersPageAsync();

        await page.AssertOnLegacyUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-user-{user.UserId}");

        await page.AssertOnLegacyEditUserPageAsync(user.UserId);

        await page.ClickButtonAsync("Deactivate");

        await page.AssertOnLegacyUsersPageAsync();

        await page.AssertFlashMessageAsync("User deactivated");
    }

    [Fact]
    public async Task ReactivateUser()
    {
        var azAdUserId = Guid.NewGuid();
        await TestData.CreateCrmUserAsync(azureAdUserId: azAdUserId, dqtRoles: ["CRM Helpdesk"]);
        var user = await TestData.CreateUserAsync(active: false, azureAdUserId: azAdUserId, role: UserRoles.Administrator);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToLegacyUsersPageAsync();

        await page.AssertOnLegacyUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-user-{user.UserId}");

        await page.AssertOnLegacyEditUserPageAsync(user.UserId);

        await page.ClickButtonAsync("Reactivate");

        await page.AssertOnLegacyEditUserPageAsync(user.UserId);

        await page.AssertFlashMessageAsync("User reactivated");
    }
}
