using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace TeachingRecordSystem.AuthorizeAccess;

public class AuthorizeAccessOptions
{
    public required bool ShowDebugPages { get; set; }

    private bool _initialized;

    // One Login signing keys
    private JsonWebKeySet? _jwks;
    private IReadOnlyDictionary<string, SigningCredentials>? _signingCredentials;

    [Required]
    public required AuthorizeAccessOptionsOneLoginSigningKey[] OneLoginSigningKeys { get; set; }

    public JsonWebKeySet GetOneLoginSigningKeysJwks()
    {
        EnsureInitialized();

        return _jwks;
    }

    [MemberNotNull(nameof(_jwks), nameof(_signingCredentials))]
    private void EnsureInitialized()
    {
        if (_initialized)
        {
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
            return;
#pragma warning restore CS8774 // Member must have a non-null value when exiting.
        }

        var jwks = new JsonWebKeySet();
        var signingCredentials = new Dictionary<string, SigningCredentials>();

        foreach (var key in OneLoginSigningKeys)
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(key.PrivateKeyPem);

            var publicSecurityKey = new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: false)) { KeyId = key.KeyId };
            var jsonWebKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(publicSecurityKey);
            jsonWebKey.Use = "sig";
            jsonWebKey.D = null;

            if (jsonWebKey.HasPrivateKey)
            {
                throw new InvalidOperationException("The JSON Web Key should not contain a private key.");
            }

            jwks.Keys.Add(jsonWebKey);

            var privateSecurityKey = new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true)) { KeyId = key.KeyId };
            var signingCredential = new SigningCredentials(privateSecurityKey, SecurityAlgorithms.RsaSha256);
            signingCredentials.Add(key.KeyId, signingCredential);
        }

        _jwks = jwks;
        _signingCredentials = signingCredentials;
        _initialized = true;
    }
}

public class AuthorizeAccessOptionsOneLoginSigningKey
{
    [Required]
    public required string KeyId { get; set; }
    [Required]
    public required string PrivateKeyPem { get; set; }
}
