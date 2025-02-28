namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class DegreeType
{
    public const int NameMaxLength = 200;

    public required Guid DegreeTypeId { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; init; }
}
