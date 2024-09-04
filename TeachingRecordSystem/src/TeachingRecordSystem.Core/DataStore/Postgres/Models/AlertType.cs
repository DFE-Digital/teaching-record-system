namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class AlertType
{
    public const int NameMaxLength = 200;
    public const int DqtSanctionCodeMaxLength = 5;
    public const string AlertCategoryIdIndexName = "ix_alert_types_alert_category_id";
    public const string AlertCategoryForeignKeyName = "fk_alert_types_alert_category";

    public required Guid AlertTypeId { get; init; }
    public required Guid AlertCategoryId { get; init; }
    public required string Name { get; init; }
    public required string? DqtSanctionCode { get; init; }
    public required ProhibitionLevel ProhibitionLevel { get; init; }
}
