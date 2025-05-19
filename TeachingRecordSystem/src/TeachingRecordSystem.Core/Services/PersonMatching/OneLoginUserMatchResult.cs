namespace TeachingRecordSystem.Core.Services.PersonMatching;

public record OneLoginUserMatchResult(
    Guid PersonId,
    string Trn,
    IReadOnlyCollection<KeyValuePair<PersonMatchedAttribute, string>> MatchedAttributes);
