namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record MatchPersonResult(
    Guid PersonId,
    IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributes);
