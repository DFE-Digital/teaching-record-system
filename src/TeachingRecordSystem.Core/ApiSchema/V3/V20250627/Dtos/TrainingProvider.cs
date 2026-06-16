using PostgresModels = TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public record TrainingProvider
{
    public required string Ukprn { get; init; }
    public required string Name { get; init; }

    public static TrainingProvider Create(PostgresModels.TrainingProvider source) => new()
    {
        Ukprn = source.Ukprn!,
        Name = source.Name
    };
}
