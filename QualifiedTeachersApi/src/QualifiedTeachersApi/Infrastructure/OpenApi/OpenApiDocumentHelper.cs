namespace QualifiedTeachersApi.Infrastructure.OpenApi;

public static class OpenApiDocumentHelper
{
    public static string GetDocumentName(string version) => $"Qualified Teachers API {version}";

    public static string GetDocumentPath(string version) => $"/swagger/{version}.json";
}
