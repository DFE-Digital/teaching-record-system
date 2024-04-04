namespace TeachingRecordSystem.Api.V3.VNext.ApiModels;

public record Alert
{
    public required Guid AlertId { get; init; }
    public required AlertTypeInfo AlertType { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
