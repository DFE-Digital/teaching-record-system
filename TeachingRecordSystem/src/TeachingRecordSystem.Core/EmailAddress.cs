using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace TeachingRecordSystem.Core;

/// <summary>
/// Represents a valid email address (code ported from Notify).
/// </summary>
[DebuggerDisplay("{_normalizedValue}")]
public sealed class EmailAddress : IEquatable<EmailAddress>, IParsable<EmailAddress>
{
    public const int EmailAddressMaxLength = 200;

    private const string ValidLocalChars = @"a-zA-Z0-9.!#$%&'*+/=?^_`{|}~\-";
    private const string EmailRegexPattern = @"^[" + ValidLocalChars + @"]+@([^.@][^@\s]+)$";

    private const string ObscureZeroWidthWhitespace = "\u180E\u200B\u200C\u200D\u2060\uFEFF";
    private const string ObscureFullWidthWhitespace = "\u00A0\u202F";

    private static readonly Regex _hostnamePartRegex = new Regex(@"^(xn|[a-z0-9]+)(-?-[a-z0-9]+)*$", RegexOptions.IgnoreCase);
    private static readonly Regex _tldPartRegex = new Regex(@"^([a-z]{2,63}|xn--([a-z0-9]+-)*[a-z0-9]+)$", RegexOptions.IgnoreCase);

    private readonly string _normalizedValue;

    private EmailAddress(string normalizedValue)
    {
        _normalizedValue = normalizedValue;
    }

    public static EmailAddress Parse(string? emailAddress)
    {
        if (!TryParseCore(emailAddress, out var result, out var error))
        {
            throw error;
        }

        return result;
    }

    public static bool TryParse(string? emailAddress, [MaybeNullWhen(false)] out EmailAddress result) =>
        TryParseCore(emailAddress, out result, out _);

    static EmailAddress IParsable<EmailAddress>.Parse(string? s, IFormatProvider? provider) => Parse(s);

    static bool IParsable<EmailAddress>.TryParse(
        string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out EmailAddress result)
    {
        if (s is null)
        {
            result = null;
            return false;
        }

        return TryParse(s, out result);
    }

    private static bool TryParseCore(
        string? emailAddress,
        [MaybeNullWhen(false)] out EmailAddress result,
        [MaybeNullWhen(true)] out FormatException error)
    {
        if (emailAddress is null)
        {
            error = new FormatException("Email address must have a value.");
            result = default;
            return false;
        }

        var normalizedEmailAddress = StripAndRemoveObscureWhitespace(emailAddress);
        Match match = Regex.Match(normalizedEmailAddress, EmailRegexPattern);

        if (normalizedEmailAddress.Length > 320)
        {
            error = new FormatException("Email address too long.");
            result = default;
            return false;
        }

        // not an email
        if (!match.Success || normalizedEmailAddress.Contains(".."))
        {
            error = new FormatException("Not a valid email address.");
            result = default;
            return false;
        }

        string hostname = match.Groups[1].Value;

        // idna = "Internationalized domain name" - this mapping converts unicode into its accurate ascii
        // representation as the web uses. '例え.テスト'.encode('idna') == b'xn--r8jz45g.xn--zckzah'
        try
        {
            hostname = new IdnMapping().GetAscii(hostname);
        }
        catch (ArgumentException)
        {
            error = new FormatException("Failed to convert to ASCII representation.");
            result = default;
            return false;
        }

        string[] parts = hostname.Split('.');

        if (hostname.Length > 253 || parts.Length < 2)
        {
            error = new FormatException("Hostname invalid length.");
            result = default;
            return false;
        }

        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part) || part.Length > 63 || !_hostnamePartRegex.IsMatch(part))
            {
                error = new FormatException("Invalid hostname.");
                result = default;
                return false;
            }
        }

        // if the part after the last . is not a valid TLD then bail out
        if (!_tldPartRegex.IsMatch(parts[^1]))
        {
            error = new FormatException("Invalid top level domain");
            result = default;
            return false;
        }

        result = new EmailAddress(normalizedEmailAddress);
        error = default;
        return true;

        static string StripAndRemoveObscureWhitespace(string value)
        {
            if (value == "")
            {
                return "";
            }

            foreach (char character in ObscureZeroWidthWhitespace + ObscureFullWidthWhitespace)
            {
                value = value.Replace(character.ToString(), "");
            }

            return value.Trim();
        }
    }

    public bool Equals(EmailAddress? other) => other is not null && _normalizedValue.Equals(other._normalizedValue);

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is EmailAddress other && Equals(other);

    public override int GetHashCode() => _normalizedValue.GetHashCode();

    public override string ToString() => _normalizedValue;

    public static bool operator ==(EmailAddress left, EmailAddress right) =>
        (left is null && right is null) ||
        (left is not null && right is not null && left.Equals(right));

    public static bool operator !=(EmailAddress left, EmailAddress right) => !(left == right);
}
