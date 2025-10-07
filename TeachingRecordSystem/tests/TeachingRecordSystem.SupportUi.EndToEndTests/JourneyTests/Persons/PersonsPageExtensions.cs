using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public static class PersonsPageExtensions
{
    public static Task GoToPersonAlertsPageAsync(this IPage page, Guid personId) =>
        page.GotoAsync($"/persons/{personId}/alerts");

    public static Task GoToPersonDetailPageAsync(this IPage page, Guid personId) =>
        page.GotoAsync($"/persons/{personId}");

    public static Task GoToPersonAddPersonPageAsync(this IPage page) =>
        page.GotoAsync($"/persons/add");

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

    public static Task AssertOnAddPersonIndexPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/persons/add");

    public static Task AssertOnAddPersonPersonalDetailsPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/persons/add/personal-details");

    public static Task AssertOnAddPersonReasonPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/persons/add/reason");

    public static Task AssertOnAddPersonCheckAnswersPageAsync(this IPage page) =>
        page.WaitForUrlPathAsync($"/persons/add/check-answers");

    public static Task AssertOnPersonSetStatusChangeReasonPageAsync(this IPage page, Guid personId, PersonStatus targetStatus) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/set-status/{targetStatus}/change-reason");

    public static Task AssertOnPersonSetStatusCheckAnswersPageAsync(this IPage page, Guid personId, PersonStatus targetStatus) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/set-status/{targetStatus}/check-answers");

    public static Task AssertOnMergePersonEnterTrnPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/merge/enter-trn");

    public static Task AssertOnMergePersonMatchesPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/merge/matches");

    public static Task AssertOnMergePersonMergePageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/merge/merge");

    public static Task AssertOnMergePersonReasonPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/merge/reason");

    public static Task AssertOnMergePersonCheckAnswersPageAsync(this IPage page, Guid personId) =>
        page.WaitForUrlPathAsync($"/persons/{personId}/merge/check-answers");
}
