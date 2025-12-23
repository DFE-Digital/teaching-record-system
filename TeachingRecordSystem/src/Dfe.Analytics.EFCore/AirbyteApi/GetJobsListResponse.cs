namespace Dfe.Analytics.EFCore.AirbyteApi;

public record GetJobsListResponse
{
    public required IReadOnlyCollection<GetJobsListResponseDataItem> Data { get; init; }
}
