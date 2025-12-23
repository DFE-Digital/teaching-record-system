namespace Dfe.Analytics.EFCore.AirbyteApi;

public record TriggerJobResponse
{
    public required long JobId { get; init; }
    public required string Status { get; init; }
    public required string JobType { get; init; }
}
