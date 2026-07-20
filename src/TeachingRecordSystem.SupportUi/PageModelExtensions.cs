using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi;

public static class PageModelExtensions
{
    public static string GetReturnUrlOrDefault(this PageModel pageModel, string defaultReturnUrl)
    {
        var returnUrl = pageModel.Request.Query["returnUrl"].ToString();

        if (!string.IsNullOrEmpty(returnUrl) && pageModel.Url.IsLocalUrl(returnUrl))
        {
            return returnUrl;
        }

        return defaultReturnUrl;
    }

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
