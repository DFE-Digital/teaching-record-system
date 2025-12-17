using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;

public record VerifiedOnlyWithMatchesOutcomeOptions
{
    public required SupportTask SupportTask { get; init; }
    public required OneLoginIdVerificationNotConnectingReason NotConnectingReason { get; init; }
    public required string? NotConnectingAdditionalDetails { get; init; }
}
