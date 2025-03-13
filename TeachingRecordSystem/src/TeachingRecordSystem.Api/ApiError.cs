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
        public static int UnexpectedInductionStatus => 10032;
        public static int StaleRequest => 10033;
        public static int PersonDoesNotHaveQts => 10034;
        public static int InvalidInductionStatus => 10035;
        public static int InductionStartDateIsRequired => 10036;
        public static int InductionCompletedDateIsRequired => 10037;
        public static int InductionStartDateIsNotPermitted => 10038;
        public static int InductionCompletedDateIsNotPermitted => 10039;
        public static int ForbiddenForAppropriateBody => 10040;
        public static int PiiUpdatesForbidden => 10041;
        public static int PiiUpdatesForbiddenPersonHasQts => 10042;
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

    public static ApiError StaleRequest(DateTime timestamp) =>
        new(ErrorCodes.StaleRequest, "Request is stale.", $"Timestamp: {timestamp:yyyy-MM-dd}");

    public static ApiError PersonDoesNotHaveQts(string trn) =>
        new(ErrorCodes.PersonDoesNotHaveQts, "Person does not have QTS.", $"TRN: '{trn}'");

    public static ApiError InvalidInductionStatus(InductionStatus status) =>
        new(ErrorCodes.InvalidInductionStatus, "Invalid induction status.", $"Status: '{status}'");

    public static ApiError InductionStartDateIsRequired(InductionStatus status) =>
        new(ErrorCodes.InductionStartDateIsRequired, "Induction start date is required.", $"Status: '{status}'");

    public static ApiError InductionStartDateIsNotPermitted(InductionStatus status) =>
        new(ErrorCodes.InductionStartDateIsNotPermitted, "Induction start date is not permitted.", $"Status: '{status}'");

    public static ApiError InductionCompletedDateIsRequired(InductionStatus status) =>
        new(ErrorCodes.InductionCompletedDateIsRequired, "Induction completed date is required.", $"Status: '{status}'");

    public static ApiError InductionCompletedDateIsNotPermitted(InductionStatus status) =>
        new(ErrorCodes.InductionCompletedDateIsNotPermitted, "Induction completed date is not permitted.", $"Status: '{status}'");

    public static ApiError ForbiddenForAppropriateBody() =>
        new(ErrorCodes.ForbiddenForAppropriateBody, "Forbidden.", "");

    public static ApiError PiiUpdatesForbidden() =>
        new(ErrorCodes.PiiUpdatesForbidden, "Updates to PII data is not permitted.", "");

    public static ApiError PiiUpdatesForbiddenPersonHasQts() =>
        new(ErrorCodes.PiiUpdatesForbiddenPersonHasQts, "Updates to PII data is not permitted. Person has QTS.", "");

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
