using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public class WebhookOptions
{
    [Required]
    public required string CanonicalDomain { get; set; }
}
