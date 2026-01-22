using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserRecordMatching;

public record ConnectedOutcomeOptions
{
    public required SupportTask SupportTask { get; init; }
    public required Guid MatchedPersonId { get; init; }
    public required IEnumerable<KeyValuePair<PersonMatchedAttribute, string>> MatchedAttributes { get; init; }
}
