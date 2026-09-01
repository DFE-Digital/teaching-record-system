using Microsoft.Playwright;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.OneLogins;

public static class OneLoginExtensions
{
    extension(IPage page)
    {
        public Task GoToOneLoginDetailPageAsync(string subject) =>
            page.GotoAsync($"/one-logins/{subject}");

        public Task AssertOnOneLoginDetailPageAsync(string subject) =>
            page.WaitForUrlPathAsync($"/one-logins/{subject}");

        public Task ClickDisconnectRecordButtonAsync() =>
            page.ClickLinkForElementWithTestIdAsync("disconnect-record-button");
    }
}
