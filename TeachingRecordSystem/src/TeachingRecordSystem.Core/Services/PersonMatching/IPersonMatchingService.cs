namespace TeachingRecordSystem.Core.Services.PersonMatching;

public interface IPersonMatchingService
{
    Task<(Guid PersonId, string Trn)?> Match(MatchRequest request);
    Task<IReadOnlyCollection<SuggestedMatch>> GetSuggestedMatches(GetSuggestedMatchesRequest request);
}
