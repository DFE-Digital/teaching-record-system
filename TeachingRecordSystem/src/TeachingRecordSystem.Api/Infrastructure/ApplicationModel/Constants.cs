namespace TeachingRecordSystem.Api.Infrastructure.ApplicationModel;

public static class Constants
{
    public static object VersionPropertyKey { get; } = "Version";
    public static object DeclaredMinorVersionPropertyKey { get; } = "MinorVersion";
    public static object MinorVersionsPropertyKey { get; } = typeof(ApiMinorVersionsMetadata);
}
