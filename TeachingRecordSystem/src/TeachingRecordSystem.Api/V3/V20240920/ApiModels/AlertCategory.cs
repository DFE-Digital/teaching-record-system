namespace TeachingRecordSystem.Api.V3.V20240920.ApiModels;

[AutoMap(typeof(Core.SharedModels.AlertCategory))]
public record AlertCategory
{
    public required Guid AlertCategoryId { get; init; }
    public required string Name { get; init; }
}
