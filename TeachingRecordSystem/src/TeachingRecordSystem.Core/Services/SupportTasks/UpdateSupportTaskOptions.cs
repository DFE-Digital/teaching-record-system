using OneOf;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.Services.SupportTasks;

public record UpdateSupportTaskOptions
{
    public required OneOf<SupportTask, string> SupportTask { get; init; }
    public required SupportTaskStatus Status { get; init; }
    public string? Comments { get; init; }
    public string? RejectionReason { get; init; }
    public Option<SavedJourneyState?> SavedJourneyState { get; init; }
}

public record UpdateSupportTaskOptions<TData> : UpdateSupportTaskOptions
{
    public required Func<TData, TData> UpdateData { get; init; }
}
