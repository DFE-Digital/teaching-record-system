using System.ComponentModel.DataAnnotations;

namespace QualifiedTeachersApi.Services.AccessYourQualifications;

public class AccessYourQualificationsOptions
{
    [Required]
    public required string BaseAddress { get; init; }
}
