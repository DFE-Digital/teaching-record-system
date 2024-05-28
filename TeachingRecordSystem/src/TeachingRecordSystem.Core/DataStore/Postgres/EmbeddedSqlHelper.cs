using Microsoft.EntityFrameworkCore.Migrations;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

internal static class EmbeddedSqlHelper
{
    public static string GetProcedureSql(string fileName) =>
        GetEmbeddedSql($"Procedures.{fileName}", "procedure");

    public static string GetTriggerSql(string fileName) =>
        GetEmbeddedSql($"Triggers.{fileName}", "trigger");

    public static void Procedure(this MigrationBuilder migrationBuilder, string fileName) =>
        migrationBuilder.Sql(GetProcedureSql(fileName));

    public static void Trigger(this MigrationBuilder migrationBuilder, string fileName) =>
        migrationBuilder.Sql(GetTriggerSql(fileName));

    private static string GetEmbeddedSql(string relativePath, string resourceType)
    {
        var resourceName = $"{typeof(TrsDbContext).Namespace}.{relativePath}";
        using var resourceStream = typeof(TrsDbContext).Assembly.GetManifestResourceStream(resourceName) ??
            throw new ArgumentException($"Could not find {resourceType} '{resourceName}'.");
        using var reader = new StreamReader(resourceStream);
        return reader.ReadToEnd();
    }
}
