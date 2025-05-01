namespace TeachingRecordSystem.Core.Services.PersonMatching;

public interface IPersonMatchingService
{
    Task<MatchResult?> MatchAsync(MatchRequest request);
    Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedMatchesAsync(GetSuggestedMatchesRequest request);
    Task<IReadOnlyCollection<KeyValuePair<PersonMatchedAttribute, string>>> GetMatchedAttributesAsync(GetSuggestedMatchesRequest request, Guid personId);
}
