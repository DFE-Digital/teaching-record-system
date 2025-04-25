using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.PersonMatching;

public interface IPersonMatchingService
{
    Task<OneLoginMatchResult?> MatchOneLoginUserAsync(OneLoginUserMatchRequest request);
    Task<IReadOnlyList<SuggestedMatch>> GetSuggestedOneLoginUserMatchesAsync(GetSuggestedOneLoginUserMatchesRequest request);
    Task<IReadOnlyList<KeyValuePair<OneLoginUserMatchedAttribute, string>>> GetMatchedAttributesAsync(GetSuggestedOneLoginUserMatchesRequest request, Guid personId);
    Task<IReadOnlyList<TrnRequestMatchResult>> MatchFromTrnRequestAsync(TrnRequestMetadata request);
}
