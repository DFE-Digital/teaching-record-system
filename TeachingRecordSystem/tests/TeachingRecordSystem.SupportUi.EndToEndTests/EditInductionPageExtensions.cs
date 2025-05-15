using Microsoft.Playwright;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.EndToEndTests;

public static class EditInductionPageExtensions
{
    public static Task GoToPersonInductionPageAsync(this IPage page, Guid personId)
    {
        return page.GotoAsync($"/persons/{personId}/induction");
    }

    public static Task ClickEditInductionStatusPageAsync(this IPage page)
    {
        return page.GetByTestId($"change-induction-status").ClickAsync();
    }

    public static Task ClickEditInductionStartDatePageAsync(this IPage page)
    {
        return page.GetByTestId($"change-induction-start-date").ClickAsync();
    }
    public static Task ClickEditInductionCompletedDatePageAsync(this IPage page)
    {
        return page.GetByTestId($"change-induction-completed-date").ClickAsync();
    }
    public static Task ClickEditInductionExemptionReasonPageAsync(this IPage page)
    {
        return page.GetByTestId($"change-induction-exemption-reason").ClickAsync();
    }

    public static Task AssertOnPersonInductionPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/induction");
    }

    public static Task AssertOnEditInductionStatusPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/status");
    }

    public static Task AssertOnEditInductionExemptionReasonPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/exemption-reasons");
    }

    public static Task AssertOnEditInductionStartDatePageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/start-date");
    }

    public static Task AssertOnEditInductionCompletedDatePageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/date-completed");
    }

    public static Task AssertOnEditInductionChangeReasonPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/change-reason");
    }

    public static Task AssertOnEditInductionCheckYourAnswersPageAsync(this IPage page, Guid personId)
    {
        return page.WaitForUrlPathAsync($"/persons/{personId}/edit-induction/check-answers");
    }

    public static Task AssertInductionStatusSelected(this IPage page, InductionStatus status)
    {
        var radioButton = page.Locator($"input[type='radio'][value='{status.ToString()}']");
        return radioButton.Locator("xpath=following-sibling::label").IsCheckedAsync();
    }

    public static Task SelectStatusAsync(this IPage page, InductionStatus status)
    {
        var radioButton = page.Locator($"input[type='radio'][value='{status.ToString()}']");
        return radioButton.Locator("xpath=following-sibling::label").ClickAsync();
    }

    public static Task SelectChangeReasonAsync(this IPage page, InductionChangeReasonOption reason)
    {
        var radioButton = page.Locator($"input[type='radio'][value='{reason.ToString()}']");
        return radioButton.Locator("xpath=following-sibling::label").ClickAsync();
    }

    public static Task SelectExemptionReasonAsync(this IPage page, Guid exemptionReasonId)
    {
        var checkbox = page.Locator($"input[type='checkbox'][value='{exemptionReasonId}']");
        return checkbox.Locator("xpath=following-sibling::label").ClickAsync();
    }
}
