namespace TeachingRecordSystem.Api;

public static class Constants
{
    public static IReadOnlyCollection<int> AllVersions { get; } = [1, 2, 3];

    public static IReadOnlyCollection<string> AllV3MinorVersions { get; } =
    [
        V3MinorVersions.V20240101
    ];

    public static string DefaultV3MinorVersion => V3MinorVersions.V20240101;

    public static class V3MinorVersions
    {
        public const string V20240101 = "20240101";
    }
}
