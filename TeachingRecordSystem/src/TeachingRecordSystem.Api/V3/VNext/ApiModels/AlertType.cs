namespace TeachingRecordSystem.Api.V3.VNext.ApiModels;

[AutoMap(typeof(Core.SharedModels.AlertType))]
public record AlertType
{
    public required Guid AlertTypeId { get; init; }
    public required AlertCategory AlertCategory { get; init; }
    public required string Name { get; init; }
}
