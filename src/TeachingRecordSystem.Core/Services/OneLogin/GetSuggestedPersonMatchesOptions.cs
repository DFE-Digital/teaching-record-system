namespace TeachingRecordSystem.Core.Services.OneLogin;

public record GetSuggestedPersonMatchesOptions(
    IEnumerable<string[]> Names,
    IEnumerable<DateOnly> DatesOfBirth,
    string? EmailAddress,
    string? NationalInsuranceNumber,
    string? Trn,
    string? TrnTokenTrnHint);
