using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Establishments;

public class GiasOptions
{
    [Required]
    public required string BaseDownloadAddress { get; init; }

    [Required]
    public required string RefreshEstablishmentsJobSchedule { get; init; }
}
