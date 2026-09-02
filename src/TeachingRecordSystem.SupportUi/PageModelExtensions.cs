using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi;

public static class PageModelExtensions
{
    extension(PageModel pageModel)
    {
        public string? GetReturnUrl()
        {
            var returnUrl = pageModel.Request.Query["returnUrl"].ToString();

            if (!string.IsNullOrEmpty(returnUrl) && pageModel.Url.IsLocalUrl(returnUrl))
            {
                return returnUrl;
            }

            return null;
        }

        public string GetReturnUrlOrDefault(string defaultReturnUrl) =>
            pageModel.GetReturnUrl() ?? defaultReturnUrl;

        public PageResult PageWithErrors() => new PageResult() { StatusCode = StatusCodes.Status400BadRequest };

        public SavedJourneyState CreateSavedJourneyState<T>(string pageName,
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
}
