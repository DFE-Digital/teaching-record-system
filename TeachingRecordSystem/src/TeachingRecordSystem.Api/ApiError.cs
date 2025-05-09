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
        public static int InvalidRouteType => 10043;
        public static int InvalidProfessionalStatusStatus => 10044;
        public static int InvalidTrainingSubjectReference => 10045;
        public static int InvalidTrainingAgeSpecialism => 10046;
        public static int InvalidTrainingCountryReference => 10047;
        public static int InvalidTrainingProviderUkprn => 10048;
        public static int InvalidDegreeType => 10049;
        public static int InvalidInductionExemptionReason => 10050;
        public static int UpdatesNotAllowedForRouteType => 10051;
        public static int RouteToProfessionalStatusAlreadyAwarded => 10052;
        public static int MultipleQtsRecords => 10053;
        public static int UnableToChangeRouteType => 10054;
        public static int UnableToChangeFailProfessionalStatusStatus => 10055;
        public static int UnableToChangeWithdrawnProfessionalStatusStatus => 10056;
        public static int PiiUpdatesForbiddenPersonHasEyts => 10057;
        public static int PersonInactive => 10058;
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

    public static ApiError PersonInactive(string trn)
    {
        var title = $"Person is inactive.";

        return new ApiError(ErrorCodes.PersonInactive, title);
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

    public static ApiError InvalidRouteType(Guid routeTypeId) =>
        new(ErrorCodes.InvalidRouteType, "Invalid route type.", $"Route type: '{routeTypeId}'");

    public static ApiError InvalidProfessionalStatusStatus(string status) =>
        new(ErrorCodes.InvalidProfessionalStatusStatus, "Invalid professional status status.", $"Status: '{status}'");

    public static ApiError InvalidTrainingSubjectReference(string trainingSubjectReference) =>
        new(ErrorCodes.InvalidTrainingSubjectReference, "Invalid training subject reference.", $"Training subject reference: '{trainingSubjectReference}'");

    public static ApiError InvalidTrainingAgeSpecialism(string trainingAgeSpecialism) =>
        new(ErrorCodes.InvalidTrainingAgeSpecialism, "Invalid training age specialism.", $"Training age specialism: '{trainingAgeSpecialism}'");

    public static ApiError InvalidTrainingCountryReference(string trainingCountryReference) =>
        new(ErrorCodes.InvalidTrainingCountryReference, "Invalid training country reference.", $"Training country reference: '{trainingCountryReference}'");

    public static ApiError InvalidTrainingProviderUkprn(string trainingProviderUkprn) =>
        new(ErrorCodes.InvalidTrainingProviderUkprn, "Invalid training provider UKPRN.", $"Training provider UKPRN: '{trainingProviderUkprn}'");

    public static ApiError InvalidDegreeType(Guid degreeTypeId) =>
        new(ErrorCodes.InvalidDegreeType, "Invalid degree type.", $"Degree type: '{degreeTypeId}'");

    public static ApiError InvalidInductionExemptionReason(Guid inductionExemptionReasonId) =>
        new(ErrorCodes.InvalidInductionExemptionReason, "Invalid induction exemption reason.", $"Induction exemption reason: '{inductionExemptionReasonId}'");

    public static ApiError UpdatesNotAllowedForRouteType(Guid routeTypeId) =>
        new(ErrorCodes.UpdatesNotAllowedForRouteType, "Updates not allowed for route type.", $"Route type: '{routeTypeId}'");

    public static ApiError RouteToProfessionalStatusAlreadyAwarded() =>
        new(ErrorCodes.RouteToProfessionalStatusAlreadyAwarded, "Route to professional status already awarded.", "");

    public static ApiError MultipleQtsRecords() =>
        new(ErrorCodes.MultipleQtsRecords, "Multiple QTS records found.", "");

    public static ApiError UnableToChangeRouteType() =>
        new(ErrorCodes.UnableToChangeRouteType, "Unable to change route type.", "");

    public static ApiError UnableToChangeFailProfessionalStatusStatus() =>
        new(ErrorCodes.UnableToChangeFailProfessionalStatusStatus, "Unable to change fail professional status status.", "");

    public static ApiError UnableToChangeWithdrawnProfessionalStatusStatus() =>
        new(ErrorCodes.UnableToChangeWithdrawnProfessionalStatusStatus, "Unable to change withdrawn professional status status.", "");

    public static ApiError PiiUpdatesForbiddenPersonHasEyts() =>
    new(ErrorCodes.PiiUpdatesForbiddenPersonHasEyts, "Updates to PII data is not permitted. Person has EYTS.", "");

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
