using Microsoft.Playwright;
using TeachingRecordSystem.Core.Services.Persons;

namespace TeachingRecordSystem.SupportUi.EndToEndTests.JourneyTests.Persons;

public static class InductionsPageExtensions
{
    extension(IPage page)
    {
        public Task GoToPersonInductionPageAsync(Guid personId)
        {
            return page.GotoAsync($"/persons/{personId}/induction");
        }

        public Task ClickEditInductionStatusPageAsync()
        {
            return page.GetByTestId($"change-induction-status").ClickAsync();
        }

        public Task ClickEditInductionStartDatePageAsync()
        {
            return page.GetByTestId($"change-induction-start-date").ClickAsync();
        }

        public Task ClickEditInductionCompletedDatePageAsync()
        {
            return page.GetByTestId($"change-induction-completed-date").ClickAsync();
        }

        public Task ClickEditInductionExemptionReasonPageAsync()
        {
            return page.GetByTestId($"change-induction-exemption-reason").ClickAsync();
        }

        public Task AssertOnPersonInductionPageAsync(Guid personId)
        {
            return page.WaitForUrlPathAsync($"/persons/{personId}/induction");
        }

        public Task AssertOnEditInductionStatusPageAsync(Guid personId)
        {
            return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/status");
        }

        public Task AssertOnEditInductionExemptionReasonPageAsync(Guid personId)
        {
            return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/exemption-reasons");
        }

        public Task AssertOnEditInductionStartDatePageAsync(Guid personId)
        {
            return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/start-date");
        }

        public Task AssertOnEditInductionCompletedDatePageAsync(Guid personId)
        {
            return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/date-completed");
        }

        public Task AssertOnEditInductionChangeReasonPageAsync(Guid personId)
        {
            return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/reason");
        }

        public Task AssertOnEditInductionCheckYourAnswersPageAsync(Guid personId)
        {
            return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/check-answers");
        }

        public Task AssertInductionStatusSelectedAsync(InductionStatus status)
        {
            var radioButton = page.Locator($"input[type='radio'][value='{status}']");
            return radioButton.Locator("xpath=following-sibling::label").IsCheckedAsync();
        }

        public Task SelectStatusAsync(InductionStatus status)
        {
            var radioButton = page.Locator($"input[type='radio'][value='{status}']");
            return radioButton.Locator("xpath=following-sibling::label").ClickAsync();
        }

        public Task SelectChangeReasonAsync(PersonInductionChangeReason reason)
        {
            var radioButton = page.Locator($"input[type='radio'][value='{reason}']");
            return radioButton.Locator("xpath=following-sibling::label").ClickAsync();
        }

        public Task SelectExemptionReasonAsync(Guid exemptionReasonId)
        {
            var checkbox = page.Locator($"input[type='checkbox'][value='{exemptionReasonId}']");
            return checkbox.Locator("xpath=following-sibling::label").ClickAsync();
        }
    }
}
