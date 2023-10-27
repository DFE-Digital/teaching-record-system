using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Dqt.Services.TrsDataSync;

public class TrsDataSyncServiceOptions
{
    [Required]
    public required int PollIntervalSeconds { get; set; }

    [Required]
    public required string[] Entities { get; set; }

    [Required]
    public required string CrmConnectionString { get; set; }

    [Required]
    public required bool ProcessAllEntityTypesConcurrently { get; set; }

    [Required]
    public required bool IgnoreInvalidData { get; set; }

    [Required]
    public required bool RunService { get; set; }
}
