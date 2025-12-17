using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;

public record VerifiedAndConnectedOutcomeOptions
{
    public required SupportTask SupportTask { get; init; }
    public required Guid MatchedPersonId { get; init; }
    public required IEnumerable<PersonMatchedAttribute> MatchedAttributeTypes { get; init; }
}
