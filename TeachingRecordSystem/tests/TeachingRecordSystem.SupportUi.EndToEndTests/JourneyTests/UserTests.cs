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
    public async Task DeactivateUser()
    {
        var azAdUserId = Guid.NewGuid();
        var user = await TestData.CreateUserAsync(azureAdUserId: azAdUserId, role: UserRoles.AccessManager);

        await using var context = await HostFixture.CreateBrowserContext();
        var page = await context.NewPageAsync();

        await page.GoToUsersPageAsync();

        await page.AssertOnUsersPageAsync();

        await page.ClickLinkForElementWithTestIdAsync($"edit-user-{user.UserId}");

        await page.AssertOnEditUserPageAsync(user.UserId);

        await page.ClickButtonAsync("Deactivate user");

        await page.AssertOnEditUserDeactivatePageAsync(user.UserId);

        await page.SetCheckedAsync("legend:has-text('Reason for deactivating user') + .govuk-radios label:has-text('They no longer need access')", true);
        await page.SetCheckedAsync("legend:has-text('Do you have more information?') + .govuk-radios label:has-text('No')", true);
        await page.SetCheckedAsync("legend:has-text('Do you have evidence to upload?') + .govuk-radios label:has-text('No')", true);

        await page.ClickButtonAsync("Continue");

        await page.AssertOnUsersPageAsync();

        await page.AssertFlashMessageAsync(expectedMessage: $"{user.Name}’s account has been deactivated.");
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

        await page.AssertFlashMessageAsync(expectedMessage: $"{user.Name}’s account has been reactivated.");
    }
}
