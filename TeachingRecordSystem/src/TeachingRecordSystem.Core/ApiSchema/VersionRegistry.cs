using Microsoft.Extensions.Configuration;

namespace TeachingRecordSystem.Core.ApiSchema;

public static class VersionRegistry
{
    public const string MinorVersionHeaderName = "X-Api-Version";
    public const string VNextVersion = "Next";

    public static IReadOnlyCollection<int> AllVersions { get; } = [1, 2, 3];

    public static IReadOnlyCollection<string> AllV3MinorVersions { get; } =
    [
        V3MinorVersions.V20240101,
        V3MinorVersions.V20240307,
        V3MinorVersions.V20240412,
        V3MinorVersions.V20240416,
        V3MinorVersions.V20240606,
        V3MinorVersions.V20240814,
        V3MinorVersions.V20240912,
        V3MinorVersions.V20240920,
        V3MinorVersions.V20250203,
        V3MinorVersions.V20250327,
        V3MinorVersions.V20250425,
        V3MinorVersions.V20250627,
        V3MinorVersions.V20250804,
        V3MinorVersions.V20250905,
        V3MinorVersions.V20260120,
        V3MinorVersions.VNext
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
                        if (minorVersion != VNextVersion || allowVNextEndpoints)
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
        public const string V20240412 = "20240412";
        public const string V20240416 = "20240416";
        public const string V20240606 = "20240606";
        public const string V20240814 = "20240814";
        public const string V20240912 = "20240912";
        public const string V20240920 = "20240920";
        public const string V20250203 = "20250203";
        public const string V20250327 = "20250327";
        public const string V20250425 = "20250425";
        public const string V20250627 = "20250627";
        public const string V20250804 = "20250804";
        public const string V20250905 = "20250905";
        public const string V20260120 = "20260120";
        public const string VNext = VNextVersion;
    }
}
