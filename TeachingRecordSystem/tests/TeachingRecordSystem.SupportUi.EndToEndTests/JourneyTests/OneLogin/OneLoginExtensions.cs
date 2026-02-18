using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.OneLogin;

public static class OneLoginExtensions
{
    public static Task GoToDisconnectOneLoginAsync(this IPage page, Guid personId, string subject)
    {
        return page.GotoAsync($"/persons/{personId}/disconnect-one-login/{subject}");
    }

    public static Task AssertOnDisconnectOneLoginIndexPageAsync(this IPage page, Guid personId, string subject)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/disconnect-one-login/{subject}");
    }

    public static Task AssertOnDisconnectOneLoginVerifiedPageAsync(this IPage page, Guid personId, string subject)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/disconnect-one-login/{subject}/verified");
    }

    public static Task AssertOnDisconnectOneLoginCheckYourAnswersPageAsync(this IPage page, Guid personId, string subject)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/disconnect-one-login/{subject}/check-answers");
    }
}
