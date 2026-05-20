namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

public record AlertCategory
{
    public required Guid AlertCategoryId { get; init; }
    public required string Name { get; init; }
}
