using System.ComponentModel.DataAnnotations;

namespace Dfe.Analytics.EFCore.AirbyteApi;

public class AirbyteOptions
{
    [Required]
    public required string ApiBaseUrl { get; set;}

    [Required]
    public required string ClientId { get; set; }

    [Required]
    public required string ClientSecret { get; set; }
}
