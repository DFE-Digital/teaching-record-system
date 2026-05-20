namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record Alert
{
    public required Guid AlertId { get; init; }
    public required AlertType AlertType { get; init; }
    public required string? Details { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
