using EntityFrameworkCore.Projectables;

namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Alert
{
    public const string AlertTypeIdIndexName = "ix_alerts_alert_type_id";
    public const string AlertTypeForeignKeyName = "fk_alerts_alert_type";
    public const string PersonIdIndexName = "ix_alerts_person_id";
    public const string PersonForeignKeyName = "fk_alerts_person";

    public required Guid AlertId { get; init; }
    public AlertType AlertType { get; } = null!;
    public required Guid AlertTypeId { get; init; }
    public required Guid PersonId { get; init; }
    public required string? Details { get; init; }
    public required string? ExternalLink { get; set; }
    public required DateOnly? StartDate { get; set; }
    public required DateOnly? EndDate { get; set; }
    public required DateTime CreatedOn { get; init; }
    public required DateTime UpdatedOn { get; set; }
    public DateTime? DeletedOn { get; set; }
    [Projectable] public bool IsOpen => EndDate == null;

    public Guid? DqtSanctionId { get; set; }
    public DateTime? DqtFirstSync { get; set; }
    public DateTime? DqtLastSync { get; set; }
    public int? DqtState { get; set; }
    public DateTime? DqtCreatedOn { get; set; }
    public DateTime? DqtModifiedOn { get; set; }
}
