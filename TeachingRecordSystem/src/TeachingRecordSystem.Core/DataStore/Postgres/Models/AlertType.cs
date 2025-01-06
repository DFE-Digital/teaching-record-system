namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class AlertType
{
    public const int NameMaxLength = 200;
    public const int DqtSanctionCodeMaxLength = 5;
    public const string AlertCategoryIdIndexName = "ix_alert_types_alert_category_id";
    public const string AlertCategoryForeignKeyName = "fk_alert_types_alert_category";
    public const string DisplayOrderIndexName = "ix_alert_types_display_order";

    public static Guid DbsAlertTypeId { get; } = new("40794ea8-eda2-40a8-a26a-5f447aae6c99");

    public required Guid AlertTypeId { get; init; }
    public required Guid AlertCategoryId { get; init; }
    public AlertCategory AlertCategory { get; } = null!;
    public required string Name { get; init; }
    public required string? DqtSanctionCode { get; init; }
    public required ProhibitionLevel ProhibitionLevel { get; init; }
    public required bool InternalOnly { get; init; }
    public required bool IsActive { get; init; }
    public int? DisplayOrder { get; init; }

    public bool IsDbsAlertType => AlertTypeId == DbsAlertTypeId;
}
