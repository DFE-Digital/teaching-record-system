using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.OneLogins;

public static class DisconnectPersonExtensions
{
    public static Task GoToDisconnectPersonAsync(this IPage page, string oneLoginSubject, Guid personId)
    {
        return page.GotoAsync($"/one-logins/{oneLoginSubject}/disconnect-person/{personId}");
    }

    public static Task AssertOnDisconnectPersonIndexPageAsync(this IPage page, string oneLoginSubject, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/one-logins/{oneLoginSubject}/disconnect-person/{personId}");
    }

    public static Task AssertOnDisconnectPersonVerifiedPageAsync(this IPage page, string oneLoginSubject, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/one-logins/{oneLoginSubject}/disconnect-person/{personId}/verified");
    }

    public static Task AssertOnDisconnectPersonCheckYourAnswersPageAsync(this IPage page, string oneLoginSubject, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/one-logins/{oneLoginSubject}/disconnect-person/{personId}/check-answers");
    }
}
