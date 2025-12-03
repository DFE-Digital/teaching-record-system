namespace TeachingRecordSystem.Core.Services.SupportTasks;

public record UpdateSupportTaskOptions<TData>
{
    public required string SupportTaskReference { get; init; }
    public required Func<TData, TData> UpdateData { get; init; }
    public required SupportTaskStatus Status { get; init; }
    public required string? Comments { get; init; }
}
