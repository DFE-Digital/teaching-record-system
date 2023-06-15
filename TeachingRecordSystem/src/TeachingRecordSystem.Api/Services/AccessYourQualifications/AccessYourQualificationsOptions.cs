using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Api.Services.AccessYourQualifications;

public class AccessYourQualificationsOptions
{
    [Required]
    public required string BaseAddress { get; init; }
}
