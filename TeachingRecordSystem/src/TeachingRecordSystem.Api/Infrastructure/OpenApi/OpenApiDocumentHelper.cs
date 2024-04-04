namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public static class OpenApiDocumentHelper
{
    public const string DocumentRouteTemplate = "/swagger/{documentName}.json";
    public const string Title = "Teaching Record System API";

    public static string GetDocumentName(int version, string? minorVersion) => GetVersionName(version, minorVersion);

    public static (int Version, string? MinorVersion) GetVersionsFromVersionName(string name)
    {
        var parts = name.Split('_');
        var majorVersion = int.Parse(parts[0][1..]);
        return parts.Length == 2 ? (majorVersion, parts[1]) : (majorVersion, null);
    }

    public static string GetVersionName(int version, string? minorVersion) => $"v{version}" + (minorVersion is not null ? $"_{minorVersion}" : "");
}
