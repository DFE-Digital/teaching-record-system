namespace TeachingRecordSystem.Core;

public static class StringHelper
{
    public static string JoinNonEmpty(char seperator, params string?[] values)
        => string.Join(seperator, values.Where(v => !string.IsNullOrEmpty(v)));
}
