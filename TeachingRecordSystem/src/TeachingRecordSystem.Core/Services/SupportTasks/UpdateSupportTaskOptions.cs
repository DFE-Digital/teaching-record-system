using OneOf;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks;

public record UpdateSupportTaskOptions<TData>
{
    public required OneOf<SupportTask, string> SupportTask { get; init; }
    public required Func<TData, TData> UpdateData { get; init; }
    public required SupportTaskStatus Status { get; init; }
    public string? Comments { get; init; }
    public string? RejectionReason { get; init; }
}
