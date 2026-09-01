using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Users;

public static class UsersPageExtensions
{
    extension(IPage page)
    {
        public Task GoToApplicationUsersPageAsync() =>
            page.GotoAsync($"/application-users");

        public Task GoToLegacyUsersPageAsync() =>
            page.GotoAsync($"/legacy-users");

        public Task GoToUsersPageAsync() =>
            page.GotoAsync($"/users");

        public Task AssertOnLegacyUsersPageAsync() =>
            page.WaitForUrlPathAsync($"/legacy-users");

        public Task AssertOnAddLegacyUserPageAsync() =>
            page.WaitForUrlPathAsync($"/legacy-users/add");

        public Task AssertOnLegacyAddUserConfirmPageAsync() =>
            page.WaitForUrlPathAsync($"/legacy-users/add/confirm");

        public Task AssertOnLegacyEditUserPageAsync(Guid userId) =>
            page.WaitForUrlPathAsync($"/legacy-users/{userId}");

        public Task AssertOnUsersPageAsync() =>
            page.WaitForUrlPathAsync($"/users");

        public Task AssertOnAddUserPageAsync() =>
            page.WaitForUrlPathAsync($"/users/add");

        public Task AssertOnAddUserConfirmPageAsync() =>
            page.WaitForUrlPathAsync($"/users/add/confirm");

        public Task AssertOnEditUserPageAsync(Guid userId) =>
            page.WaitForUrlPathAsync($"/users/{userId}");

        public Task AssertOnEditUserDeactivatePageAsync(Guid userId) =>
            page.WaitForUrlPathAsync($"/users/{userId}/deactivate");

        public Task AssertOnApplicationUsersPageAsync() =>
            page.WaitForUrlPathAsync($"/application-users");

        public Task AssertOnAddApplicationUserPageAsync() =>
            page.WaitForUrlPathAsync($"/application-users/add");

        public Task AssertOnEditApplicationUserPageAsync(Guid applicationUserId) =>
            page.WaitForUrlPathAsync($"/application-users/{applicationUserId}");
    }
}
