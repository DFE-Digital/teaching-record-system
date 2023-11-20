using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.GetAnIdentityApi;

public class GetAnIdentityOptions
{
    [Required]
    public required string TokenEndpoint { get; set; }

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }

    [Required]
    public required string BaseAddress { get; set; }

    [Required]
    public required string WebHookClientSecret { get; set; }
}
