namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record TrainingProvider
{
    public required string Ukprn { get; init; }
    public required string Name { get; init; }

    public static TrainingProvider? FromModel(Core.DataStore.Postgres.Models.TrainingProvider? model) =>
        model is null ?
            null :
            new()
            {
                Ukprn = model.Ukprn!,
                Name = model.Name
            };
}
