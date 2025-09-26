using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TeachingRecordSystem.Core;

/// <summary>
/// Represents a valid email address (code ported from Notify).
/// </summary>
[DebuggerDisplay("{NormalizedValue}")]
public sealed partial class EmailAddress : IEquatable<EmailAddress>, IParsable<EmailAddress>
{
    public const int EmailAddressMaxLength = 200;

    private const string ValidLocalChars = @"a-zA-Z0-9.!#$%&'*+/=?^_`{|}~\-";
    private const string EmailRegexPattern = @$"^[{ValidLocalChars}]+@([^.@][^@\s]+)$";
    private const string HostnamePartRegexPattern = @"^(xn|[a-z0-9]+)(-?-[a-z0-9]+)*$";
    private const string TldPartRegexPattern = @"^([a-z]{2,63}|xn--([a-z0-9]+-)*[a-z0-9]+)$";

    private const string ObscureZeroWidthWhitespace = "\u180E\u200B\u200C\u200D\u2060\uFEFF";
    private const string ObscureFullWidthWhitespace = "\u00A0\u202F";

    [JsonInclude]
    private string NormalizedValue { get; }

    [JsonConstructor]
    private EmailAddress(string normalizedValue)
    {
        NormalizedValue = normalizedValue;
    }

    public string ToDisplayString() => NormalizedValue;

    public override string ToString() => NormalizedValue;

    public bool Equals(EmailAddress? other) =>
        other is not null && NormalizedValue.Equals(other.NormalizedValue, StringComparison.Ordinal);

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is EmailAddress other && Equals(other);

    public override int GetHashCode() =>
        NormalizedValue.GetHashCode();

    public static bool operator ==(EmailAddress? left, EmailAddress? right) =>
        (left is null && right is null) ||
        (left is not null && right is not null && left.Equals(right));

    public static bool operator !=(EmailAddress? left, EmailAddress? right) =>
        !(left == right);

    public static explicit operator EmailAddress?(string? value) =>
        value is null ? null : Parse(value);

    public static explicit operator string?(EmailAddress? value) =>
        value is null ? null : value.ToString();

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
        Match match = ValidEmailPattern().Match(normalizedEmailAddress);

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
            if (string.IsNullOrEmpty(part) || part.Length > 63 || !HostNamePartPattern().IsMatch(part))
            {
                error = new FormatException("Invalid hostname.");
                result = default;
                return false;
            }
        }

        // if the part after the last . is not a valid TLD then bail out
        if (!TldPartPattern().IsMatch(parts[^1]))
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

    [GeneratedRegex(EmailRegexPattern)]
    private static partial Regex ValidEmailPattern();

    [GeneratedRegex(HostnamePartRegexPattern, RegexOptions.IgnoreCase)]
    private static partial Regex HostNamePartPattern();

    [GeneratedRegex(TldPartRegexPattern, RegexOptions.IgnoreCase)]
    private static partial Regex TldPartPattern();
}

public class EmailAddressConverter : ValueConverter<EmailAddress, string>
{
    public EmailAddressConverter()
        : base(v => v.ToString(), v => EmailAddress.Parse(v))
    {
    }
}
