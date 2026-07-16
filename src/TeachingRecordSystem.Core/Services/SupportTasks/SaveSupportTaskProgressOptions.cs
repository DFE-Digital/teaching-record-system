namespace TeachingRecordSystem.Core.Services.SupportTasks;

public record SaveSupportTaskProgressOptions
{
    public required string SupportTaskReference { get; init; }
    public required SavedJourneyState SavedJourneyState { get; init; }
}
