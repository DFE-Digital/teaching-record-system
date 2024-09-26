namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class AlertCategory
{
    public const int NameMaxLength = 200;

    public required Guid AlertCategoryId { get; init; }
    public required string Name { get; init; }
    public required int DisplayOrder { get; init; }
    public List<AlertType>? AlertTypes { get; set; }
}
