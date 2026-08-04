using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.WebCommon.Validation;

public static class PageModelValidationExtensions
{
    public static async Task ThrowIfInvalidAsync<T>(this T pageModel, IValidator<T>? validator = null)
        where T : PageModel
    {
        var result = validator is not null ? await validator.ValidateAsync(pageModel) : new ValidationResult();

        // Errors already in ModelState (e.g. from model binding, or added by the page itself) should stop us too,
        // but they're not added to the exception; FluentValidationExceptionFilter copies the exception's errors
        // into ModelState, which would duplicate them.
        if (!result.IsValid || !pageModel.ModelState.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}
