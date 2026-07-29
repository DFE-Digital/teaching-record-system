namespace TeachingRecordSystem.Core.Services.SupportTasks;

public record UpdateZendeskUrlsOptions
{
    public required string SupportTaskReference { get; init; }
    public required IEnumerable<string> ZendeskUrls { get; init; }
}
