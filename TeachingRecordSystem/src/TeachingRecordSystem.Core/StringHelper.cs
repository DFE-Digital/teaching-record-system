namespace TeachingRecordSystem.Core;

public static class StringHelper
{
    public static string JoinNonEmpty(char separator, params string?[] values)
        => string.Join(separator, values.Where(v => !string.IsNullOrEmpty(v)));
}
