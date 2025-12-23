namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record PotentialMatch(
    Guid PersonId,
    string Trn,
    string? EmailAddress,
    string FirstName,
    string? MiddleName,
    string LastName,
    DateOnly? DateOfBirth,
    string? NationalInsuranceNumber,
    PersonMatchedAttribute[] MatchedAttributes);
