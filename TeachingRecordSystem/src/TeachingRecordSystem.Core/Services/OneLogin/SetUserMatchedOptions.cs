namespace TeachingRecordSystem.Core.Services.OneLogin;

public record SetUserMatchedOptions
{
    public required string OneLoginUserSubject { get; init; }
    public required Guid MatchedPersonId { get; init; }
    public required OneLoginUserMatchRoute MatchRoute { get; init; }
    public required IEnumerable<KeyValuePair<PersonMatchedAttribute, string>>? MatchedAttributes { get; init; }
}
