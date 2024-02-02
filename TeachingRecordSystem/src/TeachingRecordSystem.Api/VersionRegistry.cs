namespace TeachingRecordSystem.Api;

public static class VersionRegistry
{
    public const string MinorVersionHeaderName = "X-Api-Version";

    public static IReadOnlyCollection<int> AllVersions { get; } = [1, 2, 3];

    public static IReadOnlyCollection<string> AllV3MinorVersions { get; } =
    [
        V3MinorVersions.V20240101,
    ];

    public static string DefaultV3MinorVersion => V3MinorVersions.V20240101;

    public static IReadOnlyCollection<(int Version, string? MinorVersion)> GetAllVersions()
    {
        return Core().AsReadOnly();

        static IEnumerable<(int Version, string? MinorVersion)> Core()
        {
            foreach (var version in AllVersions)
            {
                if (version == 3)
                {
                    foreach (var minorVersion in AllV3MinorVersions)
                    {
                        yield return (version, minorVersion);
                    }
                }
                else
                {
                    yield return (version, null);
                }
            }
        }
    }

    public static class V3MinorVersions
    {
        public const string V20240101 = "20240101";
    }
}
