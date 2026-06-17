using PostgresModels = TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

public record AlertCategory
{
    public required Guid AlertCategoryId { get; init; }
    public required string Name { get; init; }

    public static AlertCategory Create(PostgresModels.AlertCategory source) => new()
    {
        AlertCategoryId = source.AlertCategoryId,
        Name = source.Name
    };
}
