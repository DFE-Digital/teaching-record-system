namespace TeachingRecordSystem.Core.ApiSchema.V3.VNext.Dtos;

public record TrainingCountry
{
    public required string Reference { get; init; }
    public required string Name { get; init; }

    public static TrainingCountry? FromModel(Core.DataStore.Postgres.Models.Country? model) =>
        model is null ?
            null :
            new()
            {
                Reference = model.CountryId,
                Name = model.Name
            };
}
