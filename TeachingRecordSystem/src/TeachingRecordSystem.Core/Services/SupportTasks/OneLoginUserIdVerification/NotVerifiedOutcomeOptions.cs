using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserIdVerification;

public record NotVerifiedOutcomeOptions
{
    public required SupportTask SupportTask { get; init; }
    public required OneLoginIdVerificationRejectReason RejectReason { get; init; }
    public required string? RejectionAdditionalDetails { get; init; }
}
