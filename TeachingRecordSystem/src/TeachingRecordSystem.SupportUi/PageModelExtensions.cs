using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi;

public static class PageModelExtensions
{
    public static PageResult PageWithErrors(this PageModel pageModel) => new PageResult() { StatusCode = StatusCodes.Status400BadRequest };

    public static SavedJourneyState CreateSavedJourneyState<T>(this PageModel pageModel, string pageName, T state)
        where T : notnull
    {
        var modelStateValues = pageModel.ModelState.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value?.AttemptedValue);

        return new SavedJourneyState(
            pageName,
            modelStateValues,
            state,
            typeof(T));
    }

    public static bool TryApplySavedModelStateValues(this PageModel pageModel, string pageName, SavedJourneyState? savedJourneyState)
    {
        if (savedJourneyState is null)
        {
            return false;
        }

        if (savedJourneyState.PageName != pageName)
        {
            return false;
        }

        foreach (var (key, value) in savedJourneyState.ModelStateValues)
        {
            pageModel.ModelState.SetModelValue(key, value, value);
        }

        return true;
    }
}
