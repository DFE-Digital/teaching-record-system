namespace TeachingRecordSystem.Api.V3.VNext.ApiModels;

public record AlertCategoryInfo
{
    public required Guid AlertCategoryId { get; init; }
    public required string Name { get; init; }
}
