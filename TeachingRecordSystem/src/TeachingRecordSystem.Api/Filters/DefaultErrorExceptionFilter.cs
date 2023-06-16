using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Api.Validation;

namespace TeachingRecordSystem.Api.Filters;

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
