namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public static class OpenApiDocumentHelper
{
    public const string Title = "Teaching Record System API";

    public static string GetDocumentName(int version) => GetVersionName(version);

    public static string GetDocumentPath(int version) => $"/swagger/{GetVersionName(version)}.json";

    public static string GetVersionName(int version) => $"v{version}";
}
