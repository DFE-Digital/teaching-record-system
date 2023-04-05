#nullable disable
using Microsoft.AspNetCore.Mvc.Filters;
using QualifiedTeachersApi.Validation;

namespace QualifiedTeachersApi.Filters;

public class DefaultErrorExceptionFilter : IExceptionFilter
{
    public DefaultErrorExceptionFilter(int statusCode)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ErrorException ex)
        {
            context.Result = ex.ToResult(StatusCode);
            context.ExceptionHandled = true;
        }
    }
}
