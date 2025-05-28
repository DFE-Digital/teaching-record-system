namespace TeachingRecordSystem.Core;

public static class StringExtensions
{
    private static readonly char[] Vowels = ['a', 'e', 'i', 'o', 'u'];

    public static string ToLowerInvariantFirstLetter(this string text)
    {
        return text.Length > 1
            ? text.Substring(0, 1).ToLowerInvariant() + text.Substring(1)
            : text.ToLowerInvariant();
    }

    public static string WithIndefiniteArticle(this string text)
    {
        var article = Vowels.Any(v => text.StartsWith(v)) ? "an" : "a";

        return $"{article} {text}";
    }
}
