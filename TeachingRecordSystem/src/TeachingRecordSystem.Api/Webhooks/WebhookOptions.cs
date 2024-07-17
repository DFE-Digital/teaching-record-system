using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Api.Webhooks;

public class WebhookOptions
{
    [Required]
    public required string CanonicalDomain { get; set; }
}
