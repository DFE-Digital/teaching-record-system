namespace TeachingRecordSystem.Core.Services.SupportTasks.TrnRequests;

public record CompleteManualChecksNeededSupportTaskOptions
{
    public required string SupportTaskReference { get; init; }
}
