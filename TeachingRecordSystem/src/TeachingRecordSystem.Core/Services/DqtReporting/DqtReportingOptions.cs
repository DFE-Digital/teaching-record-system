using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.DqtReporting;

public class DqtReportingOptions
{
    [Required]
    public required int PollIntervalSeconds { get; set; }

    [Required]
    public required string[] Entities { get; set; }

    [Required]
    public required string CrmConnectionString { get; set; }

    [Required]
    public required string ReportingDbConnectionString { get; set; }

    [Required]
    public required bool ProcessAllEntityTypesConcurrently { get; set; }

    [Required]
    public required bool RunService { get; set; }
}
