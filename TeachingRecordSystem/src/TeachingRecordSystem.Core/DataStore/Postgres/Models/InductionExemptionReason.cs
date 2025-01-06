namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class InductionExemptionReason
{
    public static Guid PassedInWalesId { get; } = new("39550fa9-3147-489d-b808-4feea7f7f979");

    public required Guid InductionExemptionReasonId { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; set; }
}
