namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

public record AlertType
{
    public required Guid AlertTypeId { get; init; }
    public required AlertCategory AlertCategory { get; init; }
    public required string Name { get; init; }
}
