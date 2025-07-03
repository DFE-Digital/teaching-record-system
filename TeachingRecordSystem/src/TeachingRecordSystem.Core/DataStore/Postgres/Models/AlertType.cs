namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class AlertType
{
    public const int NameMaxLength = 200;
    public const int DqtSanctionCodeMaxLength = 5;
    public const string AlertCategoryIdIndexName = "ix_alert_types_alert_category_id";
    public const string AlertCategoryForeignKeyName = "fk_alert_types_alert_category";
    public const string DisplayOrderIndexName = "ix_alert_types_display_order";

    public static Guid InterimProhibitionBySoS { get; } = new("a414283f-7d5b-4587-83bf-f6da8c05b8d5");
    public static Guid ProhibitionBySoSMisconduct { get; } = new("ed0cd700-3fb2-4db0-9403-ba57126090ed");
    public static Guid SosDecisionNoProhibition { get; } = new("7924fe90-483c-49f8-84fc-674feddba848");

    public static Guid DbsAlertTypeId { get; } = new("40794ea8-eda2-40a8-a26a-5f447aae6c99");

    public required Guid AlertTypeId { get; init; }
    public required Guid AlertCategoryId { get; init; }
    public AlertCategory? AlertCategory { get; }
    public required string Name { get; init; }
    public required string? DqtSanctionCode { get; init; }
    public required ProhibitionLevel ProhibitionLevel { get; init; }
    public required bool InternalOnly { get; init; }
    public required bool IsActive { get; init; }
    public int? DisplayOrder { get; init; }

    public bool IsDbsAlertType => AlertTypeId == DbsAlertTypeId;
}
