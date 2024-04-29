namespace TeachingRecordSystem.Core.Services.PersonMatching;

public record SuggestedMatch(
    Guid PersonId,
    string Trn,
    string? Email,
    string FirstName,
    string? MiddleName,
    string LastName,
    DateOnly? DateOfBirth,
    string? NationalInsuranceNumber);
