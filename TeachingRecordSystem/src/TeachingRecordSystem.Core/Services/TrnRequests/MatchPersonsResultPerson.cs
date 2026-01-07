namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record MatchPersonsResultPerson(
    Guid PersonId,
    IReadOnlyCollection<PersonMatchedAttribute> MatchedAttributes);
