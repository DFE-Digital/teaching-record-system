using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TeachingRecordSystem.Core;

/// <summary>
/// Represents a valid National Insurance number.
/// </summary>
[DebuggerDisplay("{NormalizedValue}")]
public sealed partial class NationalInsuranceNumber : IEquatable<NationalInsuranceNumber>, IParsable<NationalInsuranceNumber>
{
    // Sourced from https://www.gov.uk/hmrc-internal-manuals/national-insurance-manual/nim39110
    // A NINO is made up of 2 letters, 6 numbers and a final letter, which is always A, B, C, or D
    // The characters D, F, I, Q, U, and V are not used as either the first or second letter of a NINO prefix.
    // The letter O is not used as the second letter of a prefix.
    // 2025-07-03: Added QQ as a valid prefix and F|M|U as valid postfixes as apparently TPS workforce data uses these
    private const string ValidNinoRegexPattern = "^[A-CEGHJ-PQR-TW-Z]{1}[A-CEGHJ-NPQR-TW-Z]{1}[0-9]{6}[A-DFMU]{1}$";
    // Prefixes BG, GB, KN, NK, NT, TN and ZZ are not to be used
    private static readonly string[] _invalidPrefixes = ["BG", "GB", "KN", "NK", "NT", "TN", "ZZ"];
    // It is sometimes necessary to use a Temporary Reference Number (TRN) for Individuals. The format of a TRN is 11 a1 11 11
    private const string ValidTempNinoRegexPattern = "^[0-9]{2}[A-Z]{1}[0-9]{5}$";

    [JsonInclude]
    private string NormalizedValue { get; }

    [JsonConstructor]
    private NationalInsuranceNumber(string normalizedValue)
    {
        NormalizedValue = normalizedValue;
    }

    public string ToDisplayString() =>
        string.Join(' ', NormalizedValue[0..2], NormalizedValue[2..4], NormalizedValue[4..6], NormalizedValue[6..8], NormalizedValue[8]);

    public override string ToString() => NormalizedValue;

    public bool Equals(NationalInsuranceNumber? other) =>
        other is not null && other.NormalizedValue == NormalizedValue;

    public override bool Equals(object? obj)
        => obj is NationalInsuranceNumber other && Equals(other);

    public override int GetHashCode() =>
        NormalizedValue.GetHashCode();

    public static bool operator ==(NationalInsuranceNumber? left, NationalInsuranceNumber? right) =>
        (left is null && right is null) ||
        (left is not null && right is not null && left.Equals(right));

    public static bool operator !=(NationalInsuranceNumber? left, NationalInsuranceNumber? right) =>
        !(left == right);

    public static explicit operator NationalInsuranceNumber?(string? value) =>
        value is null ? null : Parse(value);

    public static explicit operator string?(NationalInsuranceNumber? value) =>
        value is null ? null : value.ToString();

    public static NationalInsuranceNumber Parse(string s)
    {
        if (!TryParse(s, out var result))
        {
            throw new FormatException("Not a valid National Insurance number.");
        }

        return result;
    }

    public static bool TryParse(
        [NotNullWhen(true)] string? s,
        [MaybeNullWhen(false)] out NationalInsuranceNumber result)
    {
        if (s is null)
        {
            result = null;
            return false;
        }

        var normalized = Normalize(s);

        if (!ValidNinoPattern().IsMatch(normalized) && !ValidTempNinoPattern().IsMatch(normalized))
        {
            result = null;
            return false;
        }

        if (_invalidPrefixes.Any(normalized.StartsWith))
        {
            result = null;
            return false;
        }

        result = new NationalInsuranceNumber(normalized);
        return true;
    }

    static NationalInsuranceNumber IParsable<NationalInsuranceNumber>.Parse(string s, IFormatProvider? provider) =>
        Parse(s);

    static bool IParsable<NationalInsuranceNumber>.TryParse(
        [NotNullWhen(true)] string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out NationalInsuranceNumber result) =>
        TryParse(s, out result);

    [return: NotNullIfNotNull(nameof(value))]
    public static string? Normalize(string? value)
    {
        if (value is null)
        {
            return null;
        }

        return new(value.Where(c => !char.IsWhiteSpace(c) && c != '-').Select(char.ToUpperInvariant).ToArray());
    }

    [GeneratedRegex(ValidNinoRegexPattern)]
    private static partial Regex ValidNinoPattern();

    [GeneratedRegex(ValidTempNinoRegexPattern)]
    private static partial Regex ValidTempNinoPattern();
}

public class NationalInsuranceNumberConverter : ValueConverter<NationalInsuranceNumber, string>
{
    public NationalInsuranceNumberConverter()
        : base(v => v.ToString(), v => NationalInsuranceNumber.Parse(v))
    {
    }
}
