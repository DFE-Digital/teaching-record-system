namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record TrainingCountry
{
    public required string Reference { get; init; }
    public required string Name { get; init; }
}
