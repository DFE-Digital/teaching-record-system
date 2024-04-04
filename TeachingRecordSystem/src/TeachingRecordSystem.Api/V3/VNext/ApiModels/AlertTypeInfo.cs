namespace TeachingRecordSystem.Api.V3.VNext.ApiModels;

public record AlertTypeInfo
{
    public required Guid AlertTypeId { get; init; }
    public required AlertCategoryInfo AlertCategory { get; init; }
    public required string Name { get; init; }
}
