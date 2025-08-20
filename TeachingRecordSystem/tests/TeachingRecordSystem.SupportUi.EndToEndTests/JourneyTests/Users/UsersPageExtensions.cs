using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Users;

public static class UsersPageExtensions
{
    public static Task GoToApplicationUsersPageAsync(this IPage page) =>
        page.GotoAsync($"/application-users");

    public static Task GoToLegacyUsersPageAsync(this IPage page) =>
        page.GotoAsync($"/legacy-users");

    public static Task GoToUsersPageAsync(this IPage page) =>
        page.GotoAsync($"/users");

    public static Task AssertOnLegacyUsersPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/legacy-users");

    public static Task AssertOnAddLegacyUserPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/legacy-users/add");

    public static Task AssertOnLegacyAddUserConfirmPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/legacy-users/add/confirm");

    public static Task AssertOnLegacyEditUserPageAsync(this IPage page, Guid userId) =>
        page.WaitForUrlPathAsync($"/legacy-users/{userId}");

    public static Task AssertOnUsersPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/users");

    public static Task AssertOnAddUserPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/users/add");

    public static Task AssertOnAddUserConfirmPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/users/add/confirm");

    public static Task AssertOnEditUserPageAsync(this IPage page, Guid userId) =>
        page.WaitForUrlPathAsync($"/users/{userId}");

    public static Task AssertOnEditUserDeactivatePageAsync(this IPage page, Guid userId) =>
        page.WaitForUrlPathAsync($"/users/{userId}/deactivate");

    public static Task AssertOnApplicationUsersPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/application-users");

    public static Task AssertOnAddApplicationUserPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/application-users/add");

    public static Task AssertOnEditApplicationUserPageAsync(this IPage page, Guid applicationUserId) =>
        page.WaitForUrlPathAsync($"/application-users/{applicationUserId}");
}
