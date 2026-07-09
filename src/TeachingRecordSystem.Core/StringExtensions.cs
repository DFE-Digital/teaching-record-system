namespace TeachingRecordSystem.Core;

public static class StringExtensions
{
    private static readonly string[] _vowels = ["a", "e", "i", "o", "u"];

    extension(string str)
    {
        public static string JoinNonEmpty(char separator, params string?[] values) =>
            string.Join(separator, values.Where(v => !string.IsNullOrEmpty(v)));

        public string ToLowerInvariantFirstLetter()
        {
            return str.Length > 1
                ? str[..1].ToLowerInvariant() + str[1..]
                : str.ToLowerInvariant();
        }

        public string WithIndefiniteArticle()
        {
            var article = _vowels.Any(v => str.StartsWith(v, StringComparison.OrdinalIgnoreCase)) ? "an" : "a";
            return $"{article} {str}";
        }
    }
}
