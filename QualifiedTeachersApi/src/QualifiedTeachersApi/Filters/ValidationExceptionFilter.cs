using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace QualifiedTeachersApi.Filters;

public class ValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ValidationException validationException)
        {
            var validationResult = new ValidationResult(validationException.Errors);
            validationResult.AddToModelState(context.ModelState, prefix: null);

            var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            context.Result = new ObjectResult(problemDetailsFactory.CreateValidationProblemDetails(context.HttpContext, context.ModelState));

            context.ExceptionHandled = true;
        }
    }
}
