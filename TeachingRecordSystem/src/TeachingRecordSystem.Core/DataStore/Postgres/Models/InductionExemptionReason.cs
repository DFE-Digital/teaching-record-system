namespace TeachingRecordSystem.Core.DataStore.Postgres.Models;

public class InductionExemptionReason
{
    public static Guid OverseasTrainedTeacherId { get; } = new("4c97e211-10d2-4c63-8da9-b0fcebe7f2f9");
    public static Guid PassedInductionInNorthernIrelandId { get; } = new("3471ab35-e6e4-4fa9-a72b-b8bd113df591");
    public static Guid HasOrIsEligibleForFullRegistrationInScotlandId { get; } = new("a112e691-1694-46a7-8f33-5ec5b845c181");
    public static Guid PassedInWalesId { get; } = new("39550fa9-3147-489d-b808-4feea7f7f979");
    public static Guid QtlsId { get; } = new("35caa6a3-49f2-4a63-bd5a-2ba5fa9dc5db");

    public required Guid InductionExemptionReasonId { get; init; }
    public required string Name { get; init; }
    public required bool IsActive { get; set; }
}
