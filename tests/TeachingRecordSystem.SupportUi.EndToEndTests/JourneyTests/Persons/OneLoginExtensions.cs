using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public static class OneLoginExtensions
{
    extension(IPage page)
    {
        public Task GoToDisconnectOneLoginAsync(Guid personId, string subject)
        {
            return page.GotoAsync($"/persons/{personId}/disconnect-one-login/{subject}");
        }

        public Task ClickDisconnectOneLoginLinkAsync(string emailAddress)
        {
            return page.GetByTestId("associated-one-login-users")
                .GetByRole(AriaRole.Link, new() { Name = $"Disconnect {emailAddress}" })
                .ClickAsync();
        }

        public Task AssertOnDisconnectOneLoginIndexPageAsync(Guid personId, string subject)
        {
            return page.WaitForUrlPathAsync($"/persons/{personId}/disconnect-one-login/{subject}");
        }

        public Task AssertOnDisconnectOneLoginVerifiedPageAsync(Guid personId, string subject)
        {
            return page.WaitForUrlPathAsync($"/persons/{personId}/disconnect-one-login/{subject}/verified");
        }

        public Task AssertOnDisconnectOneLoginCheckYourAnswersPageAsync(Guid personId, string subject)
        {
            return page.WaitForUrlPathAsync($"/persons/{personId}/disconnect-one-login/{subject}/check-answers");
        }
    }
}
