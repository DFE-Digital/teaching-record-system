using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi;

public static class PageModelExtensions
{
    public static PageResult PageWithErrors(this PageModel pageModel) => new PageResult() { StatusCode = StatusCodes.Status400BadRequest };

    public static SavedJourneyState CreateSavedJourneyState<T>(
        this PageModel pageModel,
        string pageName,
        T state,
        params string[] excludeKeys)
        where T : notnull
    {
        var modelStateValues = pageModel.ModelState
            .Where(m => !excludeKeys.Contains(m.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.AttemptedValue);

        return new SavedJourneyState(
            pageName,
            modelStateValues,
            state,
            typeof(T));
    }
}
