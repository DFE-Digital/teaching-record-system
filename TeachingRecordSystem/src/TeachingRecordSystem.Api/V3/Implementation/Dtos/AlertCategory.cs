namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record AlertCategory
{
    public required Guid AlertCategoryId { get; init; }
    public required string Name { get; init; }
}
