using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TeachingRecordSystem.Core;

/// <summary>
/// Represents a valid mobile phone number that can be sent messages via Notify.
/// </summary>
[DebuggerDisplay("{NormalizedValue}")]
public sealed class MobileNumber : IEquatable<MobileNumber>, IParsable<MobileNumber>
{
    private static readonly char[] _ignoredCharacters =
    [
        ' ',
        '\t',
        '\n',
        '\r',
        '\f',
        '\u180E',
        '\u200B',
        '\u200C',
        '\u200D',
        '\u2060',
        '\uFEFF',
        '\u00A0',
        '\u202F',
        '(',
        ')',
        '-',
        '+'
    ];

    // From https://github.com/alphagov/notifications-utils/blob/c0144abc7bc83a36b4dec47c70972259155b8cc7/notifications_utils/international_billing_rates.yml
    private static readonly string[] _internationPrefixies =
    [
        "1",
        "7",
        "20",
        "27",
        "30",
        "31",
        "32",
        "33",
        "34",
        "36",
        "39",
        "40",
        "41",
        "43",
        "44",
        "45",
        "46",
        "47",
        "48",
        "49",
        "51",
        "52",
        "53",
        "54",
        "55",
        "56",
        "57",
        "58",
        "60",
        "61",
        "62",
        "63",
        "64",
        "65",
        "66",
        "81",
        "82",
        "84",
        "86",
        "90",
        "91",
        "92",
        "93",
        "94",
        "95",
        "98",
        "211",
        "212",
        "213",
        "216",
        "218",
        "220",
        "221",
        "222",
        "223",
        "224",
        "225",
        "226",
        "227",
        "228",
        "229",
        "230",
        "231",
        "232",
        "233",
        "234",
        "235",
        "236",
        "237",
        "238",
        "239",
        "240",
        "241",
        "242",
        "243",
        "244",
        "245",
        "246",
        "248",
        "249",
        "250",
        "251",
        "252",
        "253",
        "254",
        "255",
        "256",
        "257",
        "258",
        "260",
        "261",
        "262",
        "263",
        "264",
        "265",
        "266",
        "267",
        "268",
        "269",
        "297",
        "298",
        "299",
        "350",
        "351",
        "352",
        "353",
        "354",
        "355",
        "356",
        "357",
        "358",
        "359",
        "370",
        "371",
        "372",
        "373",
        "374",
        "375",
        "376",
        "377",
        "378",
        "380",
        "381",
        "382",
        "385",
        "386",
        "387",
        "389",
        "420",
        "421",
        "423",
        "500",
        "501",
        "502",
        "503",
        "504",
        "505",
        "506",
        "507",
        "508",
        "509",
        "590",
        "591",
        "592",
        "593",
        "594",
        "595",
        "596",
        "597",
        "598",
        "599",
        "670",
        "672",
        "673",
        "674",
        "675",
        "676",
        "677",
        "678",
        "679",
        "680",
        "682",
        "685",
        "687",
        "689",
        "691",
        "692",
        "852",
        "853",
        "855",
        "856",
        "880",
        "886",
        "960",
        "961",
        "962",
        "963",
        "964",
        "965",
        "966",
        "967",
        "968",
        "970",
        "971",
        "972",
        "973",
        "974",
        "975",
        "976",
        "977",
        "992",
        "993",
        "994",
        "995",
        "996",
        "998",
        "1242",
        "1246",
        "1264",
        "1268",
        "1284",
        "1345",
        "1441",
        "1473",
        "1649",
        "1664",
        "1684",
        "1721",
        "1758",
        "1767",
        "1784",
        "1868",
        "1869",
        "1876"
    ];

    private static readonly string _ukPrefix = "44";

    [JsonInclude]
    private string NormalizedValue { get; }

    [JsonConstructor]
    private MobileNumber(string normalizedValue)
    {
        NormalizedValue = normalizedValue;
    }

    public string ToDisplayString() =>
        NormalizedValue[..2] == "44" ? '0' + NormalizedValue[2..] : '+' + NormalizedValue;

    public override string ToString() => NormalizedValue;

    public bool Equals(MobileNumber? other) =>
        other is not null && NormalizedValue.Equals(other.NormalizedValue);

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        obj is MobileNumber other && Equals(other);

    public override int GetHashCode() =>
        NormalizedValue.GetHashCode();

    public static bool operator ==(MobileNumber? left, MobileNumber? right) =>
        (left is null && right is null) ||
        (left is not null && right is not null && left.Equals(right));

    public static bool operator !=(MobileNumber? left, MobileNumber? right) =>
        !(left == right);

    public static explicit operator MobileNumber?(string? value) =>
        value is null ? null : Parse(value);

    public static explicit operator string?(MobileNumber? value) =>
        value is null ? null : value.ToString();

    public static MobileNumber Parse(string number)
    {
        if (!TryParseCore(number, out var result, out var error))
        {
            throw error;
        }

        return result;
    }

    public static bool TryParse(string number, [MaybeNullWhen(false)] out MobileNumber result) =>
        TryParseCore(number, out result, out _);

    static MobileNumber IParsable<MobileNumber>.Parse(string s, IFormatProvider? provider) => Parse(s);

    static bool IParsable<MobileNumber>.TryParse(
        string? s,
        IFormatProvider? provider,
        [MaybeNullWhen(false)] out MobileNumber result)
    {
        if (s is null)
        {
            result = null;
            return false;
        }

        return TryParse(s, out result);
    }

    private static bool TryParseCore(
        string number,
        [MaybeNullWhen(false)] out MobileNumber result,
        [MaybeNullWhen(true)] out FormatException error)
    {
        var normalizedNumber = new string(number.Where(c => !_ignoredCharacters.Contains(c)).ToArray()).TrimStart('0');

        if (!normalizedNumber.All(char.IsAsciiDigit))
        {
            error = new FormatException("Must not contain letters or symbols.");
            result = default;
            return false;
        }

        if (IsUkPhoneNumber(normalizedNumber))
        {
            return TryParseUkPhoneNumber(normalizedNumber, out result, out error);
        }

        if (normalizedNumber.Length < 8)
        {
            error = new FormatException("Not enough digits.");
            result = default;
            return false;
        }

        if (normalizedNumber.Length > 15)
        {
            error = new FormatException("Too many digits.");
            result = default;
            return false;
        }

        if (!HasInternationalPrefix(normalizedNumber))
        {
            error = new FormatException("Not a valid country prefix.");
            result = default;
            return false;
        }

        result = new MobileNumber(normalizedNumber);
        error = default;
        return true;

        static bool IsUkPhoneNumber(string normalizedNumber)
        {
            if (normalizedNumber.StartsWith("0") && !normalizedNumber.StartsWith("00"))
            {
                return true;
            }

            if (normalizedNumber.StartsWith(_ukPrefix) || (normalizedNumber.StartsWith("7") && normalizedNumber.Length < 11))
            {
                return true;
            }

            return false;
        }

        static bool TryParseUkPhoneNumber(
            string normalizedNumber,
            [MaybeNullWhen(false)] out MobileNumber result,
            [MaybeNullWhen(true)] out FormatException? error)
        {
            var number = normalizedNumber.TrimStart(_ukPrefix.ToCharArray()).TrimStart('0');

            if (!number.StartsWith("7"))
            {
                error = new FormatException("Not a UK mobile number.");
                result = default;
                return false;
            }

            if (number.Length > 10)
            {
                error = new FormatException("Too many digits.");
                result = default;
                return false;
            }

            if (number.Length < 10)
            {
                error = new FormatException("Not enough digits.");
                result = default;
                return false;
            }

            result = new($"{_ukPrefix}{number}");
            error = null;
            return true;
        }

        static bool HasInternationalPrefix(string number) => _internationPrefixies.Any(number.StartsWith);
    }
}

public class MobileNumberConverter : ValueConverter<MobileNumber, string>
{
    public MobileNumberConverter()
        : base(v => v.ToString(), v => MobileNumber.Parse(v))
    {
    }
}
