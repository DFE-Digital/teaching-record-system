#nullable disable
using System.ComponentModel.DataAnnotations;

namespace QualifiedTeachersApi.Services.GetAnIdentityApi;

public class GetAnIdentityOptions
{
    [Required]
    public string TokenEndpoint { get; set; }

    [Required]
    public string ClientSecret { get; init; }

    [Required]
    public string ClientId { get; init; }

    [Required]
    public string BaseAddress { get; init; }

    [Required]
    public string WebHookClientSecret { get; init; }
}
