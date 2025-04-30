namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests;

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

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPageAsync();

        await page.AssertOnUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync("add-user");

        await page.AssertOnAddUserPageAsync();

        await page.FillEmailInputAsync(testAzAdUser.Email);

        await page.ClickButtonAsync("Add user");

        await page.AssertOnAddUserConfirmPageAsync();

        await page.SetCheckedAsync("label:has-text('Support officer')", true);

        await page.ClickButtonAsync("Add user");

        await page.AssertOnUsersPageAsync();

        await page.AssertFlashMessageAsync(expectedMessage: $"{testAzAdUser.Name} has been added as a support officer.");
    }

    [Fact]
    public async Task EditUser()
    {
        var azAdUserId = Guid.NewGuid();
        var user = await TestData.CreateUserAsync(azureAdUserId: azAdUserId, role: UserRoles.AccessManager);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPageAsync();

        await page.AssertOnUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-user-{user.UserId}");

        await page.AssertOnEditUserPageAsync(user.UserId);

        await page.SetCheckedAsync("label:has-text('Support officer')", true);

        await page.ClickButtonAsync("Save changes");

        await page.AssertOnUsersPageAsync();

        await page.AssertFlashMessageAsync(expectedMessage: $"{user.Name} has been changed to a support officer.");
    }

    [Fact]
    public async Task ReactivateUser()
    {
        var azAdUserId = Guid.NewGuid();
        var user = await TestData.CreateUserAsync(active: false, azureAdUserId: azAdUserId, role: UserRoles.AccessManager);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPageAsync();

        await page.AssertOnUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-user-{user.UserId}");

        await page.AssertOnEditUserPageAsync(user.UserId);

        await page.ClickButtonAsync("Reactivate user");

        await page.AssertOnUsersPageAsync();

        await page.AssertFlashMessageAsync(expectedMessage: $"{user.Name} has been reactivated.");
    }
}
