using PostgresModels = TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20250627.Dtos;

public record DegreeType
{
    public required Guid DegreeTypeId { get; init; }
    public required string Name { get; init; }

    public static DegreeType Create(PostgresModels.DegreeType source) => new()
    {
        DegreeTypeId = source.DegreeTypeId,
        Name = source.Name
    };
}
