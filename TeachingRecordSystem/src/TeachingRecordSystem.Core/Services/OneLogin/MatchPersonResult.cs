namespace TeachingRecordSystem.Core.Services.OneLogin;

public record MatchPersonResult(
    Guid PersonId,
    string Trn,
    IReadOnlyCollection<KeyValuePair<PersonMatchedAttribute, string>> MatchedAttributes);
