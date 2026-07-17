using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record CreateManualChecksNeededSupportTaskOptions
{
    public required Person Person { get; init; }
    public required TrnRequestMetadata TrnRequest { get; init; }
}
