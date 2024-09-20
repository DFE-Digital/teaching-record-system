namespace TeachingRecordSystem.Api.V3.V20240920.ApiModels;

[AutoMap(typeof(Core.SharedModels.Alert))]
public record Alert
{
    public required Guid AlertId { get; init; }
    public required AlertType AlertType { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
