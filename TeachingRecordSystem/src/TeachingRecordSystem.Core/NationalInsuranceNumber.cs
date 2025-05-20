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
    private static readonly string[] _invalidPrefixes = ["BG", "GB", "KN", "NK", "NT", "TN", "ZZ"];

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

        if (!ValidNinoPattern().IsMatch(normalized))
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

    // Sourced from https://www.gov.uk/hmrc-internal-manuals/national-insurance-manual/nim39110
    [GeneratedRegex("^[A-CEGHJ-PR-TW-Z]{1}[A-CEGHJ-NPR-TW-Z]{1}[0-9]{6}[A-D]{1}$")]
    private static partial Regex ValidNinoPattern();
}

public class NationalInsuranceNumberConverter : ValueConverter<NationalInsuranceNumber, string>
{
    public NationalInsuranceNumberConverter()
        : base(v => v.ToString(), v => NationalInsuranceNumber.Parse(v))
    {
    }
}
