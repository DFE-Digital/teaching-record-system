using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Jobs;

public class DeleteOldEvidenceFilesJobOptions
{
    [Required]
    public required string JobSchedule { get; init; }

    [Required]
    public required int RetentionPeriodDays { get; init; }
}
