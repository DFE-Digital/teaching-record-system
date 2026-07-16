using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks.TrnRequests;

public record CreateTrnRequestSupportTaskOptions
{
    public required TrnRequestMetadata TrnRequest { get; init; }
}
