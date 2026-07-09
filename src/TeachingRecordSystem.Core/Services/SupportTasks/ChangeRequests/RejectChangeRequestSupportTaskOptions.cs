using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;

public record RejectChangeRequestSupportTaskOptions
{
    public required SupportTask SupportTask { get; init; }
    public required string RejectionReason { get; init; }
}
