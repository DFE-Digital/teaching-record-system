using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.DqtReporting;

public class DqtReportingOptions
{
    public const string DefaultTrsDbReplicationSlotName = "dqt_rep_sync_slot";

    [Required]
    public required string ReportingDbConnectionString { get; set; }

    [Required]
    public required bool RunService { get; set; }

    public required string TrsDbReplicationSlotName { get; set; } = DefaultTrsDbReplicationSlotName;
}
