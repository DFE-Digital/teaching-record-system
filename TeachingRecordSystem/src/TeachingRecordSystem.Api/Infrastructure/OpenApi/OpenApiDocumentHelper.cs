namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public static class OpenApiDocumentHelper
{
    public const string DocumentRouteTemplate = "/swagger/{documentName}.json";
    public const string Title = "Teaching Record System API";

    public static string GetDocumentName(int version, string? minorVersion) => GetVersionName(version, minorVersion);

    public static string GetVersionName(int version, string? minorVersion) => $"v{version}" + (minorVersion is not null ? $"_{minorVersion}" : "");
}
