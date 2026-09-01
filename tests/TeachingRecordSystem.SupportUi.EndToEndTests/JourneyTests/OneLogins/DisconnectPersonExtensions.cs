using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.OneLogins;

public static class DisconnectPersonExtensions
{
    extension(IPage page)
    {
        public Task GoToDisconnectPersonAsync(string oneLoginSubject, Guid personId)
        {
            return page.GotoAsync($"/one-logins/{oneLoginSubject}/disconnect-person/{personId}");
        }

        public Task AssertOnDisconnectPersonIndexPageAsync(string oneLoginSubject, Guid personId)
        {
            return page.WaitForUrlPathAsync($"/one-logins/{oneLoginSubject}/disconnect-person/{personId}");
        }

        public Task AssertOnDisconnectPersonVerifiedPageAsync(string oneLoginSubject, Guid personId)
        {
            return page.WaitForUrlPathAsync($"/one-logins/{oneLoginSubject}/disconnect-person/{personId}/verified");
        }

        public Task AssertOnDisconnectPersonCheckYourAnswersPageAsync(string oneLoginSubject, Guid personId)
        {
            return page.WaitForUrlPathAsync($"/one-logins/{oneLoginSubject}/disconnect-person/{personId}/check-answers");
        }
    }
}
