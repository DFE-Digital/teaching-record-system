using System.ComponentModel.DataAnnotations;

namespace QualifiedTeachersApi.Services.TrnGenerationApi;

public class TrnGenerationApiOptions
{
    [Required]
    public required string ApiKey { get; init; }

    [Required]
    public required string BaseAddress { get; init; }
}
