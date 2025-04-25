namespace TeachingRecordSystem.Core.Services.PersonMatching;

public record TrnRequestMatchResult(
    Guid PersonId,
    string Trn,
    bool DefiniteMatch,
    bool HasAlerts,
    bool HasQts,
    bool HasEyts,
    IReadOnlyCollection<KeyValuePair<TrnRequestMatchedAttribute, string>> MatchedAttributes);
