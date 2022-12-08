using System.ComponentModel.DataAnnotations;

namespace TeacherIdentity.AuthServer.Services.DqtApi;

public class TrnGenerationApiOptions
{
    [Required]
    public string ApiKey { get; init; }

    [Required]
    public string BaseAddress { get; init; }
}
