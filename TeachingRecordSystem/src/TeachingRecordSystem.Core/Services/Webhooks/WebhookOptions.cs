using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace TeachingRecordSystem.Core.Services.Webhooks;

public class WebhookOptions
{
    [Required]
    public required string CanonicalDomain { get; set; }

    [Required]
    public required string SigningKeyId { get; set; }

    [Required]
    public required WebhookOptionsKey[] Keys { get; set; }

    public JsonWebKeySet GetJsonWebKeySet()
    {
        // FUTURE memoize this

        var keySet = new JsonWebKeySet();

        foreach (var key in Keys)
        {
            using var certificate = X509Certificate2.CreateFromPem(key.CertificatePem);
            var securityKey = new ECDsaSecurityKey(certificate.GetECDsaPublicKey());

            var jsonWebKey = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(securityKey);
            jsonWebKey.Use = "sig";
            jsonWebKey.Alg = "ES384";
            jsonWebKey.KeyId = key.KeyId;

            var certChain = certificate.ExportCertificatePem().Split("\n")
                .Skip(1)  // Remove -----BEGIN CERTIFICATE-----
                .SkipLast(1)  // Remove -----END CERTIFICATE-----
                .Aggregate((l, r) => l + r);

            jsonWebKey.X5c.Add(certChain);

            Debug.Assert(!jsonWebKey.HasPrivateKey);

            keySet.Keys.Add(jsonWebKey);
        }

        return keySet;
    }
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
