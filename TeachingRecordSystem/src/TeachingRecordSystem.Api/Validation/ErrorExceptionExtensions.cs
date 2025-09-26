using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api.Validation;

public static class ErrorExceptionExtensions
{
    public static ObjectResult ToResult(this ErrorException ex, int statusCode)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var error = ex.Error;

        var problemDetails = new ProblemDetails()
        {
            Title = error.Title,
            Detail = error.Detail,
            Status = statusCode,
            Extensions =
            {
                { "errorCode", error.ErrorCode }
            }
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }
}
