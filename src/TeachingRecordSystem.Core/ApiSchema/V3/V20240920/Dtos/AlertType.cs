using PostgresModels = TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.Core.ApiSchema.V3.V20240920.Dtos;

public record AlertType
{
    public required Guid AlertTypeId { get; init; }
    public required AlertCategory AlertCategory { get; init; }
    public required string Name { get; init; }

    public static AlertType Create(PostgresModels.AlertType source) => new()
    {
        AlertTypeId = source.AlertTypeId,
        AlertCategory = AlertCategory.Create(source.AlertCategory!),
        Name = source.Name
    };
}
