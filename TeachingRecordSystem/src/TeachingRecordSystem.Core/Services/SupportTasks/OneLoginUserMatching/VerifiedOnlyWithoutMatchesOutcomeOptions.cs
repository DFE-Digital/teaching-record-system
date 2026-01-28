using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public record VerifiedOnlyWithoutMatchesOutcomeOptions
{
    public required SupportTask SupportTask { get; init; }
}
