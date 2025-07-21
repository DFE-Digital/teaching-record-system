namespace TeachingRecordSystem.Core.Services.PersonMatching;

public record OneLoginUserSuggestedMatch(
    Guid PersonId,
    string Trn,
    string? EmailAddress,
    string FirstName,
    string? MiddleName,
    string LastName,
    DateOnly? DateOfBirth,
    string? NationalInsuranceNumber);
