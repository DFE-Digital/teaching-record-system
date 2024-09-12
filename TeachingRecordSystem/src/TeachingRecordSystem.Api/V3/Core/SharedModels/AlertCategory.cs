namespace TeachingRecordSystem.Api.V3.Core.SharedModels;

public record AlertCategory
{
    public required Guid AlertCategoryId { get; init; }
    public required string Name { get; init; }
}
