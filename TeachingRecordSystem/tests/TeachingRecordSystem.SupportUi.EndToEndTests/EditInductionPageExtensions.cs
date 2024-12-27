using Microsoft.Playwright;

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
}
