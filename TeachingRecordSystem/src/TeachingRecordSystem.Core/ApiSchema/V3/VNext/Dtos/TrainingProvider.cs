namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record TrainingProvider
{
    public required string Ukprn { get; init; }
    public required string Name { get; init; }
}
