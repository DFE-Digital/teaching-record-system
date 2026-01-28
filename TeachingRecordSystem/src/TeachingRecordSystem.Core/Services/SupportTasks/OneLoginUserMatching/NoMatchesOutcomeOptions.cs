using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserMatching;

public record NoMatchesOutcomeOptions
{
    public required SupportTask SupportTask { get; init; }
}
