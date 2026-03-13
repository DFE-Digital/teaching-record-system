using System.Text.Json;
using System.Text.RegularExpressions;
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
        IOpenIddictApplicationStore<OpenIddictEntityFrameworkCoreApplication<Guid>> store) : base(cache, logger, options, store)
    {
    }

    protected override ValueTask<string> ObfuscateClientSecretAsync(string secret, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    protected override ValueTask<bool> ValidateClientSecretAsync(string secret, string comparand, CancellationToken cancellationToken = default) =>
        ValueTask.FromResult(secret.Equals(comparand, StringComparison.Ordinal));

    public override ValueTask<bool> ValidatePostLogoutRedirectUriAsync(
        OpenIddictEntityFrameworkCoreApplication<Guid> application,
        string uri,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(application?.PostLogoutRedirectUris) || string.IsNullOrEmpty(uri))
        {
            return ValueTask.FromResult(false);
        }

        string[] allowed;
        try
        {
            allowed = JsonSerializer.Deserialize<string[]>(application.PostLogoutRedirectUris) ?? Array.Empty<string>();
        }
        catch
        {
            allowed = Array.Empty<string>();
        }

        foreach (var pattern in allowed)
        {
            if (MatchUriPattern(pattern, uri, ignorePath: false))
            {
                return ValueTask.FromResult(true);
            }
        }

        return ValueTask.FromResult(false);
    }

    public override ValueTask<bool> ValidateRedirectUriAsync(OpenIddictEntityFrameworkCoreApplication<Guid> application, string address, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(application?.RedirectUris) || string.IsNullOrEmpty(address))
        {
            return ValueTask.FromResult(false);
        }

        string[] allowed;
        try
        {
            allowed = JsonSerializer.Deserialize<string[]>(application.RedirectUris) ?? Array.Empty<string>();
        }
        catch
        {
            allowed = Array.Empty<string>();
        }
        foreach (var pattern in allowed)
        {
            if (MatchUriPattern(pattern, address, ignorePath: false))
            {
                return ValueTask.FromResult(true);
            }
        }

        return ValueTask.FromResult(false);
    }

    private static bool MatchUriPattern(string pattern, string uri, bool ignorePath)
    {
        if (!Uri.TryCreate(pattern, UriKind.Absolute, out _))
        {
            throw new ArgumentException("A valid absolute URI must be specified.", nameof(pattern));
        }

        if (!Uri.TryCreate(uri, UriKind.Absolute, out _))
        {
            throw new ArgumentException("A valid absolute URI must be specified.", nameof(uri));
        }

        var normalizedPattern = ignorePath ? RemovePathAndQuery(pattern) : pattern;
        var normalizedUri = ignorePath ? RemovePathAndQuery(uri) : uri;

        if (normalizedPattern.Equals(normalizedUri, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (normalizedPattern.Contains("*"))
        {
            // Escape pattern then replace escaped '*' with '.*' to allow wildcard matches.
            var escaped = Regex.Escape(normalizedPattern).Replace("\\*", ".*");
            return Regex.IsMatch(normalizedUri, $"^{escaped}$", RegexOptions.IgnoreCase);
        }

        return false;

        static string RemovePathAndQuery(string address) => new Uri(address).GetLeftPart(UriPartial.Authority);
    }
}
