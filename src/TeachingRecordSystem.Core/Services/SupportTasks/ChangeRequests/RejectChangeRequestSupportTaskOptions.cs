using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;

public record RejectChangeRequestSupportTaskOptions
{
    public required SupportTask SupportTask { get; init; }
    public required ChangeRequestRejectReason RejectionReason { get; init; }
}
