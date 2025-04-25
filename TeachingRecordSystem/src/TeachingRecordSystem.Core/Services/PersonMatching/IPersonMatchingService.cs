using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.PersonMatching;

public interface IPersonMatchingService
{
    Task<OneLoginMatchResult?> MatchOneLoginUserAsync(OneLoginUserMatchRequest request);
    Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedOneLoginUserMatchesAsync(GetSuggestedOneLoginUserMatchesRequest request);
    Task<IReadOnlyCollection<KeyValuePair<OneLoginUserMatchedAttribute, string>>> GetMatchedAttributesAsync(GetSuggestedOneLoginUserMatchesRequest request, Guid personId);
    Task<IReadOnlyCollection<TrnRequestMatchResult>> MatchFromTrnRequestAsync(TrnRequestMetadata request);
}
