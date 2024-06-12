using System.Text.RegularExpressions;

namespace TeachingRecordSystem.Core;

public static partial class NationalInsuranceNumberHelper
{
    public static bool IsValid(string? value)
    {
        if (value is null)
        {
            return false;
        }

        var normalized = Normalize(value)!;

        return ValidNinoPattern().IsMatch(normalized);
    }

    [GeneratedRegex("^[A-CEGHJ-PQR-TW-Za-ceghj-pr-tw-z]{1}[A-CEGHJ-NPQR-TW-Za-ceghj-npr-tw-z]{1}[0-9]{6}[A-DFMUa-dfm]{0,1}$")]
    private static partial Regex ValidNinoPattern();

    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return new string(value.Where(c => !Char.IsWhiteSpace(c) && c != '-').ToArray());
    }
}
