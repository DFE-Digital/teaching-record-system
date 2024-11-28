using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.Api;

public sealed record ApiError(int ErrorCode, string Title, string? Detail = null)
{
    public static class ErrorCodes
    {
        public static int PersonNotFound => 10001;
        public static int SpecifiedResourceUrlDoesNotExist => 10028;
        public static int TrnRequestAlreadyCreated => 10029;
        public static int TrnRequestDoesNotExist => 10031;
    }

    public static ApiError PersonNotFound(string trn, DateOnly? dateOfBirth = null, string? nationalInsuranceNumber = null)
    {
        var title = $"Person not found.";

        var detail = $"TRN: '{trn}'";
        if (dateOfBirth is not null)
        {
            detail += $"\nDate of birth: '{dateOfBirth:yyyy-MM-dd}'";
        }
        if (!string.IsNullOrEmpty(nationalInsuranceNumber))
        {
            detail += $"\nNational insurance number: '{nationalInsuranceNumber}'";
        }

        return new ApiError(ErrorCodes.PersonNotFound, title, detail);
    }

    public static ApiError SpecifiedResourceUrlDoesNotExist(string url) =>
        new(ErrorCodes.SpecifiedResourceUrlDoesNotExist, "The specified resource does not exist.", $"URL: '{url}'");

    public static ApiError TrnRequestAlreadyCreated(string requestId) =>
        new(ErrorCodes.TrnRequestAlreadyCreated, "TRN request has already been created.", $"TRN request ID: '{requestId}'");

    public static ApiError TrnRequestDoesNotExist(string requestId) =>
        new(ErrorCodes.TrnRequestDoesNotExist, "TRN request does not exist.", $"TRN request ID: '{requestId}'");

    public IActionResult ToActionResult(int statusCode = 400)
    {
        var problemDetails = new ProblemDetails()
        {
            Title = Title,
            Detail = Detail,
            Status = statusCode,
            Extensions =
            {
                { "errorCode", ErrorCode }
            }
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }
}
