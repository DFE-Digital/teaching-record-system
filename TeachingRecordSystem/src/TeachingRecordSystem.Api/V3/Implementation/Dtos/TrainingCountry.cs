namespace TeachingRecordSystem.Api.V3.Implementation.Dtos;

public record TrainingCountry
{
    public required string Reference { get; init; }
    public required string Name { get; init; }

    public static TrainingCountry? FromModel(PostgresModels.Country? model) =>
        model is null ?
            null :
            new()
            {
                Reference = model.CountryId,
                Name = model.Name
            };
}
