namespace TeachingRecordSystem.Core;

public static class TrnHelper
{
    public static string? NormalizeTrn(string? value) =>
        string.IsNullOrEmpty(value) ? null : new(value.Where(char.IsAsciiDigit).ToArray());
}
