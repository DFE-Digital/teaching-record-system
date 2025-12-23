using Dfe.Analytics.EFCore.Description;
using Microsoft.EntityFrameworkCore;

namespace Dfe.Analytics.EFCore.Configuration;

public class ConfigurationProvider
{
    private const string NpgsqlProviderName = "Npgsql.EntityFrameworkCore.PostgreSQL";

    public DatabaseSyncConfiguration GetConfiguration(DbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext);

        ThrowIfUnsupportedProvider(dbContext);

        var tables = new List<TableSyncInfo>();

        foreach (var entityType in dbContext.Model.GetEntityTypes())
        {
            if ((entityType.FindAnnotation(AnnotationKeys.TableAnalyticsSyncMetadata)?.Value as TableSyncMetadata)?.SyncTable is not true)
            {
                continue;
            }

            var primaryKey = entityType.GetKeys().SingleOrDefault(k => k.IsPrimaryKey());
            if (primaryKey is null)
            {
                throw new InvalidOperationException($"Entity '{entityType.Name}' does not have a primary key.");
            }

            var tableName = entityType.GetTableName()!;

            var primaryKeyColumnNames = primaryKey.Properties.Select(p => p.Name);

            var columnNames = entityType.GetProperties().Select(p => p.GetColumnName());

            tables.Add(new TableSyncInfo
            {
                Name = tableName,
                PrimaryKey = new TablePrimaryKeySyncInfo { ColumnNames = primaryKeyColumnNames.ToArray() },
                Columns = columnNames.Select(cn => new ColumnSyncInfo { Name = cn }).ToArray()
            });
        }

        var configuration = new DatabaseSyncConfiguration { Tables = tables.ToArray() };
        return configuration;
    }

    private static void ThrowIfUnsupportedProvider(DbContext dbContext)
    {
        if (dbContext.Database.ProviderName is not NpgsqlProviderName)
        {
            throw new NotSupportedException($"{dbContext.Database.ProviderName} is not a supported provider.");
        }
    }
}
