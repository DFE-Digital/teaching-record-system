namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class InductionExemptionReason
{
    public static Guid PassedInWalesId { get; } = new("39550fa9-3147-489d-b808-4feea7f7f979");
    public static Guid QtlsId { get; } = new("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db");

    public required Guid InductionExemptionReasonId { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; set; }
    public required bool RouteImplicitExemption { get; set; }
}
