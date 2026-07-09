using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks.ChangeRequests;

public record CancelChangeRequestSupportTaskOptions
{
    public required SupportTask SupportTask { get; init; }
}
