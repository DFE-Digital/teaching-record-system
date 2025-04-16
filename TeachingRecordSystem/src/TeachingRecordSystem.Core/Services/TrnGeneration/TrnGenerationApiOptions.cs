using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.TrnGeneration;

public class TrnGenerationApiOptions
{
    [Required]
    public required string ApiKey { get; init; }

    [Required]
    public required string BaseAddress { get; init; }
}
