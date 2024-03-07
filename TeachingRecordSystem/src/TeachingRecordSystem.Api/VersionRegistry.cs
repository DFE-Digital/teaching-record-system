namespace TeachingRecordSystem.Api;

public static class VersionRegistry
{
    public const string MinorVersionHeaderName = "X-Api-Version";
    public const string VNextVersion = "Next";

    public static IReadOnlyCollection<int> AllVersions { get; } = [1, 2, 3];

    public static IReadOnlyCollection<string> AllV3MinorVersions { get; } =
    [
        V3MinorVersions.V20240101,
        V3MinorVersions.V20240307,
        V3MinorVersions.VNext,
    ];

    public static string DefaultV3MinorVersion => V3MinorVersions.V20240101;

    public static IReadOnlyCollection<(int Version, string? MinorVersion)> GetAllVersions(IConfiguration configuration)
    {
        return Core().AsReadOnly();

        IEnumerable<(int Version, string? MinorVersion)> Core()
        {
            var allowVNextEndpoints = configuration.GetValue<bool>("AllowVNextEndpoints");

            foreach (var version in AllVersions)
            {
                if (version == 3)
                {
                    foreach (var minorVersion in AllV3MinorVersions)
                    {
                        if (minorVersion != VersionRegistry.VNextVersion || allowVNextEndpoints)
                        {
                            yield return (version, minorVersion);
                        }
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
        public const string V20240307 = "20240307";
        public const string VNext = VNextVersion;
    }
}
