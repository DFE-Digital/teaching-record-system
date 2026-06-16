namespace TeachingRecordSystem.Api.V3.Operations.Common;

public record AlertType
{
    public required Guid AlertTypeId { get; init; }
    public required AlertCategory AlertCategory { get; init; }
    public required string Name { get; init; }
    public required string DqtSanctionCode { get; init; }
}
