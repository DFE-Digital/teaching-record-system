namespace TeachingRecordSystem.Core.Services.OneLogin;

public record SetUserVerifiedAndMatchedOptions
{
    public required string OneLoginUserSubject { get; init; }
    public required OneLoginUserVerificationRoute VerificationRoute { get; init; }
    public required DateOnly[] VerifiedDatesOfBirth { get; init; }
    public required string[][] VerifiedNames { get; init; }
    public required string? CoreIdentityClaimVc { get; init; }
    public required Guid MatchedPersonId { get; init; }
    public required OneLoginUserMatchRoute MatchRoute { get; init; }
    public required IEnumerable<KeyValuePair<PersonMatchedAttribute, string>> MatchedAttributes { get; init; }
}
