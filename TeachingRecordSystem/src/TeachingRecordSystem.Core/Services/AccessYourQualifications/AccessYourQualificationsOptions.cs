using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.AccessYourQualifications;

public class AccessYourQualificationsOptions
{
    [Required]
    public required string BaseAddress { get; init; }

    public string StartUrlPath { get; set; } = "/qualifications/start";
}
