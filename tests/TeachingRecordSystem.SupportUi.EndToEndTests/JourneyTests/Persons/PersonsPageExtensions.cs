using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public static class PersonsPageExtensions
{
    extension(IPage page)
    {
        public Task GoToPersonAlertsPageAsync(Guid personId) =>
            page.GotoAsync($"/persons/{personId}/alerts");

        public Task GoToPersonDetailPageAsync(Guid personId) =>
            page.GotoAsync($"/persons/{personId}");

        public Task GoToPersonAddPersonPageAsync() =>
            page.GotoAsync($"/persons/add");

        public Task GoToPersonQualificationsPageAsync(Guid personId) =>
            page.GotoAsync($"/persons/{personId}/qualifications");

        public Task AssertOnPersonDetailPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}");

        public Task AssertOnPersonAlertsPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/alerts");

        public Task AssertOnPersonQualificationsPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/qualifications");

        public Task AssertOnPersonEditNamePageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/edit-name");

        public Task AssertOnPersonEditNameConfirmPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/edit-name/confirm");

        public Task AssertOnPersonEditDateOfBirthPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/edit-date-of-birth");

        public Task AssertOnPersonEditDateOfBirthConfirmPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/edit-date-of-birth/confirm");

        public Task AssertOnPersonEditDetailsPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/edit-details");

        public Task AssertOnPersonEditDetailsNameChangeReasonPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/edit-details/name-change-reason");

        public Task AssertOnPersonEditDetailsOtherDetailsChangeReasonPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/edit-details/other-details-change-reason");

        public Task AssertOnPersonEditDetailsCheckAnswersPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/edit-details/check-answers");

        public Task AssertOnAddPersonIndexPageAsync() =>
            page.WaitForUrlPathAsync($"/persons/add");

        public Task AssertOnAddPersonPersonalDetailsPageAsync() =>
            page.WaitForUrlPathAsync($"/persons/add/personal-details");

        public Task AssertOnAddPersonReasonPageAsync() =>
            page.WaitForUrlPathAsync($"/persons/add/reason");

        public Task AssertOnAddPersonCheckAnswersPageAsync() =>
            page.WaitForUrlPathAsync($"/persons/add/check-answers");

        public Task AssertOnPersonSetStatusChangeReasonPageAsync(Guid personId, PersonStatus targetStatus) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/set-status/{targetStatus}/reason");

        public Task AssertOnPersonSetStatusCheckAnswersPageAsync(Guid personId, PersonStatus targetStatus) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/set-status/{targetStatus}/check-answers");

        public Task AssertOnMergePersonEnterTrnPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/merge/enter-trn");

        public Task AssertOnMergePersonMatchesPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/merge/matches");

        public Task AssertOnMergePersonMergePageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/merge/merge");

        public Task AssertOnMergePersonReasonPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/merge/reason");

        public Task AssertOnMergePersonCheckAnswersPageAsync(Guid personId) =>
            page.WaitForUrlPathAsync($"/persons/{personId}/merge/check-answers");
    }
}
