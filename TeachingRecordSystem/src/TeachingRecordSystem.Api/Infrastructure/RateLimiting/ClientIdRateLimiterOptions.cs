using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Api.Infrastructure.RateLimiting;

public class ClientIdRateLimiterOptions
{
    [Required]
    public required FixedWindowOptions DefaultRateLimit { get; set; }

    public required Dictionary<string, FixedWindowOptions> ClientRateLimits { get; set; } = [];
}
