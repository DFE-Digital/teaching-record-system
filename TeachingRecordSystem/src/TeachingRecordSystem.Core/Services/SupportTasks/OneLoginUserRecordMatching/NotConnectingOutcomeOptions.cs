using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.OneLoginUserRecordMatching;

public record NotConnectingOutcomeOptions
{
    public required SupportTask SupportTask { get; init; }
    public required OneLoginUserNotConnectingReason NotConnectingReason { get; init; }
    public required string? NotConnectingAdditionalDetails { get; init; }
}
