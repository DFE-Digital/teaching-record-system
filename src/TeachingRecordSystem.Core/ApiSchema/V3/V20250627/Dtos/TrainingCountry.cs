using PostgresModels = TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public record TrainingCountry
{
    public required string Reference { get; init; }
    public required string Name { get; init; }

    public static TrainingCountry Create(PostgresModels.Country source) => new()
    {
        Reference = source.CountryId,
        Name = source.Name
    };
}
