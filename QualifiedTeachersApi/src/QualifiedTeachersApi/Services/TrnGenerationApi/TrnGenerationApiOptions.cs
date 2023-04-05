#nullable disable
using System.ComponentModel.DataAnnotations;

namespace QualifiedTeachersApi.Services.TrnGenerationApi;

public class TrnGenerationApiOptions
{
    [Required]
    public string ApiKey { get; init; }

    [Required]
    public string BaseAddress { get; init; }
}
