namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record DegreeType
{
    public required Guid DegreeTypeId { get; init; }
    public required string Name { get; init; }
}
