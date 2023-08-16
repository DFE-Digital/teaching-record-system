namespace TeachingRecordSystem.Api.Infrastructure.OpenApi;

public static class OpenApiDocumentHelper
{
    public static string GetDocumentName(int version) => $"Teaching Record System API {GetVersionName(version)}";

    public static string GetDocumentPath(int version) => $"/swagger/{GetVersionName(version)}.json";

    public static string GetVersionName(int version) => $"v{version}";
}
