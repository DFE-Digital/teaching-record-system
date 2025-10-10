using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Infrastructure.Filters;

public class FluentValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ValidationException validationException)
        {
            new ValidationResult(validationException.Errors).AddToModelState(context.ModelState);
            context.Result = new PageResult { StatusCode = StatusCodes.Status400BadRequest };
            context.ExceptionHandled = true;
        }
    }
}
