namespace TeachingRecordSystem.Core.Services.PersonMatching;

public record OneLoginUserMatchRequest(
    IEnumerable<string[]> Names,
    IEnumerable<DateOnly> DatesOfBirth,
    string? NationalInsuranceNumber,
    string? Trn);

public record GetSuggestedOneLoginUserMatchesRequest(
    IEnumerable<string[]> Names,
    IEnumerable<DateOnly> DatesOfBirth,
    string? NationalInsuranceNumber,
    string? Trn,
    string? TrnTokenTrnHint);
