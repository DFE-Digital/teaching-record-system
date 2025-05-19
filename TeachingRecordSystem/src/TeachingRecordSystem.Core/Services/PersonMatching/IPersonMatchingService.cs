using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.PersonMatching;

public interface IPersonMatchingService
{
    Task<OneLoginUserMatchResult?> MatchOneLoginUserAsync(OneLoginUserMatchRequest request);
    Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedOneLoginUserMatchesAsync(GetSuggestedOneLoginUserMatchesRequest request);
    Task<IReadOnlyCollection<KeyValuePair<PersonMatchedAttribute, string>>> GetMatchedAttributesAsync(GetSuggestedOneLoginUserMatchesRequest request, Guid personId);
    Task<TrnRequestMatchResult> MatchFromTrnRequestAsync(TrnRequestMetadata request);
    Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedMatchesFromTrnRequestAsync(TrnRequestMetadata request);
}
