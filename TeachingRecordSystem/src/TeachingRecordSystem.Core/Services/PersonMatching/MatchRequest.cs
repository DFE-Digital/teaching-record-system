namespace TeachingRecordSystem.Core.Services.PersonMatching;

public record MatchRequest(
    IEnumerable<string[]> Names,
    IEnumerable<DateOnly> DatesOfBirth,
    string? NationalInsuranceNumber,
    string? Trn);

public record GetSuggestedMatchesRequest(
    IEnumerable<string[]> Names,
    IEnumerable<DateOnly> DatesOfBirth,
    string? NationalInsuranceNumber,
    string? Trn,
    string? TrnTokenTrnHint);
