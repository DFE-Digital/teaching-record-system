using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public static class PersonsPageExtensions
{
    public static Task GoToPersonAlertsPageAsync(this IPage page, Guid personId) =>
        page.GotoAsync($"/persons/{personId}/alerts");

    public static Task GoToPersonDetailPageAsync(this IPage page, Guid personId) =>
        page.GotoAsync($"/persons/{personId}");

    public static Task GoToPersonCreatePageAsync(this IPage page) =>
        page.GotoAsync($"/persons/create");

    public static Task GoToPersonQualificationsPageAsync(this IPage page, Guid personId) =>
        page.GotoAsync($"/persons/{personId}/qualifications");

    public static Task AssertOnPersonDetailPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}");

    public static Task AssertOnPersonAlertsPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/alerts");

    public static Task AssertOnPersonQualificationsPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/qualifications");

    public static Task AssertOnPersonEditNamePageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/edit-name");

    public static Task AssertOnPersonEditNameConfirmPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/edit-name/confirm");

    public static Task AssertOnPersonEditDateOfBirthPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/edit-date-of-birth");

    public static Task AssertOnPersonEditDateOfBirthConfirmPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/edit-date-of-birth/confirm");

    public static Task AssertOnPersonEditDetailsPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/edit-details");

    public static Task AssertOnPersonEditDetailsNameChangeReasonPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/edit-details/name-change-reason");

    public static Task AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/edit-details/other-details-change-reason");

    public static Task AssertOnPersonEditDetailsCheckAnswersPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/edit-details/check-answers");

    public static Task AssertOnPersonCreateIndexPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/persons/create");

    public static Task AssertOnPersonCreatePersonalDetailsPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/persons/create/personal-details");

    public static Task AssertOnPersonCreateCreateReasonPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/persons/create/create-reason");

    public static Task AssertOnPersonCreateCheckAnswersPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/persons/create/check-answers");

    public static Task AssertOnPersonSetStatusChangeReasonPageAsync(this IPage page, Guid personId, PersonStatus targetStatus) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/set-status/{targetStatus}/change-reason");

    public static Task AssertOnPersonSetStatusCheckAnswersPageAsync(this IPage page, Guid personId, PersonStatus targetStatus) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/set-status/{targetStatus}/check-answers");
}
