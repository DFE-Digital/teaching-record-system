using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Qualifications;

public static class QualificationsPageExtensions
{
    public static Task GoToAddMqPageAsync(this IPage page, Guid personId) =>
        page.GotoAsync($"/mqs/add?personId={personId}");

    public static Task AssertOnAddMqProviderPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/mqs/add/provider");

    public static Task AssertOnAddMqSpecialismPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/mqs/add/specialism");

    public static Task AssertOnAddMqStartDatePageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/mqs/add/start-date");

    public static Task AssertOnAddMqStatusPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/mqs/add/status");

    public static Task AssertOnAddMqCheckAnswersPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/mqs/add/check-answers");

    public static Task AssertOnEditMqProviderPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider");

    public static Task AssertOnEditMqProviderReasonPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider/reason");

    public static Task AssertOnEditMqProviderConfirmPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider/check-answers");

    public static Task AssertOnEditMqSpecialismPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism");

    public static Task AssertOnEditMqSpecialismReasonPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism/reason");

    public static Task AssertOnEditMqSpecialismConfirmPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism/check-answers");

    public static Task AssertOnEditMqStartDatePageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date");

    public static Task AssertOnEditMqStartDateReasonPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date/reason");

    public static Task AssertOnEditMqStartDateConfirmPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date/check-answers");

    public static Task AssertOnEditMqStatusPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status");

    public static Task AssertOnEditMqStatusReasonPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status/reason");

    public static Task AssertOnEditMqStatusConfirmPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status/check-answers");

    public static Task AssertOnDeleteMqPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/delete");

    public static Task AssertOnDeleteMqConfirmPageAsync(this IPage page, Guid qualificationId) =>
        page.WaitForUrlPathAsync($"/mqs/{qualificationId}/delete/check-answers");
}
