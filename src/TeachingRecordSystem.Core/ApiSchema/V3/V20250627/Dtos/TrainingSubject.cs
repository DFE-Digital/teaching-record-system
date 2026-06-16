using PostgresModels = TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public record TrainingSubject
{
    public required string Reference { get; init; }
    public required string Name { get; init; }

    public static TrainingSubject Create(PostgresModels.TrainingSubject source) => new()
    {
        Reference = source.Reference,
        Name = source.Name
    };
}
