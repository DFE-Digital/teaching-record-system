namespace TeachingRecordSystem.Core.Services.PersonMatching;

public record OneLoginMatchResult(
    Guid PersonId,
    string Trn,
    IReadOnlyCollection<KeyValuePair<OneLoginUserMatchedAttribute, string>> MatchedAttributes);
