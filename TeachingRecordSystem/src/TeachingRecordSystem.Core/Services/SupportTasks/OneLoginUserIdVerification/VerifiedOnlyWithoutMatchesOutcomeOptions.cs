using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;

public record VerifiedOnlyWithoutMatchesOutcomeOptions
{
    public required SupportTask SupportTask { get; init; }
}
