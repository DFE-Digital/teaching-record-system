using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace TeachingRecordSystem.AuthorizeAccess.Infrastructure.Oidc;

public class ApplicationManager : OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>>
{
    public ApplicationManager(
        IOpenIddictApplicationCache<OpenIddictEntityFrameworkCoreApplication<Guid>> cache,
        ILogger<OpenIddictApplicationManager<OpenIddictEntityFrameworkCoreApplication<Guid>>> logger,
        IOptionsMonitor<OpenIddictCoreOptions> options,
        IOpenIddictApplicationStoreResolver resolver) : base(cache, logger, options, resolver)
    {
    }

    protected override ValueTask<string> ObfuscateClientSecretAsync(string secret, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    protected override ValueTask<bool> ValidateClientSecretAsync(string secret, string comparand, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(secret.Equals(comparand));
}
