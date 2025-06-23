namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record TrainingSubject
{
    public required string Reference { get; init; }
    public required string Name { get; init; }

    public static TrainingSubject FromModel(Core.DataStore.Postgres.Models.TrainingSubject model) =>
        new()
        {
            Reference = model.Reference,
            Name = model.Name
        };
}
