namespace TeachingRecordSystem.Core.Services.TrnRequests;

public record CompleteManualChecksNeededSupportTaskOptions
{
    public required string SupportTaskReference { get; init; }
}
