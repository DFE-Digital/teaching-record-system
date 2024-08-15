namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class Alert
{
    public const string AlertTypeIdIndexName = "ix_alerts_alert_type_id";
    public const string AlertTypeForeignKeyName = "fk_alerts_alert_type";
    public const string PersonIdIndexName = "ix_alerts_person_id";
    public const string PersonForeignKeyName = "fk_alerts_person";

    public required Guid AlertId { get; init; }
    public required Guid AlertTypeId { get; init; }
    public required Guid PersonId { get; init; }
    public required string Details { get; init; }
    public required string? ExternalLink { get; init; }
    public required DateOnly? StartDate { get; init; }
    public required DateOnly? EndDate { get; init; }
}
