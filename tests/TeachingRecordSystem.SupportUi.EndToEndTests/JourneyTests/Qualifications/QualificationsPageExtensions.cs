using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Qualifications;

public static class QualificationsPageExtensions
{
    extension(IPage page)
    {
        public Task ClickConfirmEditButtonAsync() =>
            page.ClickButtonAsync("Confirm and update qualification");

        public Task GoToAddMqPageAsync(Guid personId) =>
            page.GotoAsync($"/mqs/add?personId={personId}");

        public Task AssertOnAddMqProviderPageAsync() =>
            page.WaitForUrlPathAsync($"/mqs/add/provider");

        public Task AssertOnAddMqSpecialismPageAsync() =>
            page.WaitForUrlPathAsync($"/mqs/add/specialism");

        public Task AssertOnAddMqStartDatePageAsync() =>
            page.WaitForUrlPathAsync($"/mqs/add/start-date");

        public Task AssertOnAddMqStatusPageAsync() =>
            page.WaitForUrlPathAsync($"/mqs/add/status");

        public Task AssertOnAddMqReasonPageAsync() =>
            page.WaitForUrlPathAsync($"/mqs/add/reason");

        public Task AssertOnAddMqCheckAnswersPageAsync() =>
            page.WaitForUrlPathAsync($"/mqs/add/check-answers");

        public Task AssertOnEditMqProviderPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider");

        public Task AssertOnEditMqProviderReasonPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider/reason");

        public Task AssertOnEditMqProviderConfirmPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/provider/check-answers");

        public Task AssertOnEditMqSpecialismPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism");

        public Task AssertOnEditMqSpecialismReasonPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism/reason");

        public Task AssertOnEditMqSpecialismConfirmPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/specialism/check-answers");

        public Task AssertOnEditMqStartDatePageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date");

        public Task AssertOnEditMqStartDateReasonPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date/reason");

        public Task AssertOnEditMqStartDateConfirmPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/start-date/check-answers");

        public Task AssertOnEditMqStatusPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status");

        public Task AssertOnEditMqStatusReasonPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status/reason");

        public Task AssertOnEditMqStatusConfirmPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/status/check-answers");

        public Task AssertOnDeleteMqPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/delete");

        public Task AssertOnDeleteMqConfirmPageAsync(Guid qualificationId) =>
            page.WaitForUrlPathAsync($"/mqs/{qualificationId}/delete/check-answers");
    }
}
