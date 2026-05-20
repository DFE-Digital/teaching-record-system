using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core;

public class AccessYourTeachingQualificationsOptions
{
    [Required]
    public required string BaseAddress { get; init; }

    public string StartUrlPath { get; set; } = "/qualifications/start";
}
