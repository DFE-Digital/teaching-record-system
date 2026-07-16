using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.Core.Services.SupportTasks.TrnRequests;

public record ResolveTrnRequestSupportTaskOptions
{
    public required string SupportTaskReference { get; init; }
    public required TrnRequestDataPersonAttributes? ResolvedAttributes { get; init; }
    public required TrnRequestDataPersonAttributes? SelectedPersonAttributes { get; init; }
    public string? Comments { get; init; }
}
