namespace Dfe.Analytics.EFCore.AirbyteApi;

public record GetJobsListResponseDataItem
{
    public required long Id { get; init; }
    public required string Status { get; init; }
    public required string JobType { get; init; }
}
