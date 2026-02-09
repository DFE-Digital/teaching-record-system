using System.Diagnostics.CodeAnalysis;

namespace TeachingRecordSystem.Core;

public static class SearchTextHelper
{
    public static bool IsDate(string searchText, out DateOnly date) =>
        DateOnly.TryParseExact(searchText, DateDisplayFormat, out date) ||
        DateOnly.TryParseExact(searchText, "d/M/yyyy", out date) ||
        DateOnly.TryParseExact(searchText, "d MMM yyyy", out date);

    public static bool IsEmailAddress(string searchText, [NotNullWhen(true)] out string? email)
    {
        var isEmail = searchText.Contains('@');
        email = isEmail ? searchText : null;
        return isEmail;
    }

    public static bool IsSupportTaskReference(string searchText) =>
        searchText.StartsWith("TRS-", StringComparison.OrdinalIgnoreCase);

    public static bool IsTrn(string searchText) =>
        searchText.Length == 7 && searchText.All(char.IsAsciiDigit);
}
