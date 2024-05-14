namespace TeachingRecordSystem.Core.Services.PersonMatching;

public interface IPersonMatchingService
{
    Task<MatchResult?> Match(MatchRequest request);
    Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedMatches(GetSuggestedMatchesRequest request);
    Task<IReadOnlyCollection<KeyValuePair<OneLoginUserMatchedAttribute, string>>> GetMatchedAttributes(GetSuggestedMatchesRequest request, Guid personId);
}
