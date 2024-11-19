using System.ComponentModel.DataAnnotations;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public class WebhookOptions
{
    [Required]
    public required string CanonicalDomain { get; set; }

    [Required]
    public required string SigningKeyId { get; set; }

    [Required]
    public required WebhookOptionsKey[] Keys { get; set; }
}

public class WebhookOptionsKey
{
    [Required]
    public required string KeyId { get; set; }
    [Required]
    public required string CertificatePem { get; set; }
    [Required]
    public required string PrivateKeyPem { get; set; }
}
