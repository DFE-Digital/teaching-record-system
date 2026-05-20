using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.OneLogins;

public static class OneLoginExtensions
{
    public static Task GoToOneLoginDetailPageAsync(this IPage page, string subject) =>
        page.GotoAsync($"/one-logins/{subject}");

    public static Task AssertOnOneLoginDetailPageAsync(this IPage page, string subject) =>
        page.WaitForUrlPathAsync($"/one-logins/{subject}");
}
