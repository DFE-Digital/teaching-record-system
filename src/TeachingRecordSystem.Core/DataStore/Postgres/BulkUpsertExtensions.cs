using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace TeachingRecordSystem.Core.DataStore.Postgres;

/// <summary>
/// Inserts entities, or updates them if a row with the same primary key already exists,
/// using <c>INSERT ... ON CONFLICT (pk) DO UPDATE</c>.
/// </summary>
/// <remarks>
/// <para>The SQL is built from the EF model, so column names, value converters and TPH discriminators are respected.
/// Entities are not tracked by the context and are not modified.</para>
/// <para>Only entity types mapped to a single table with scalar properties are supported;
/// owned types, complex types, JSON-mapped types, TPT, TPC and entity splitting throw <see cref="NotSupportedException"/>.</para>
/// </remarks>
internal static class BulkUpsertExtensions
{
    // Postgres' limit on the number of parameters in a single statement
    private const int MaxParametersPerStatement = 65_535;

    public static void BulkUpsert<T>(this DbContext dbContext, IEnumerable<T> entities)
        where T : class
    {
        foreach (var (sql, parameters) in CreateCommands(dbContext, entities))
        {
            dbContext.Database.ExecuteSqlRaw(sql, parameters);
        }
    }

    public static async Task BulkUpsertAsync<T>(this DbContext dbContext, IEnumerable<T> entities, CancellationToken cancellationToken = default)
        where T : class
    {
        foreach (var (sql, parameters) in CreateCommands(dbContext, entities))
        {
            await dbContext.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
        }
    }

    private static IEnumerable<(string Sql, DbParameter[] Parameters)> CreateCommands<T>(DbContext dbContext, IEnumerable<T> entities)
        where T : class
    {
        var sqlGenerationHelper = dbContext.GetService<ISqlGenerationHelper>();

        // Only used as a factory for parameters; never executed
        using var parameterFactory = dbContext.Database.GetDbConnection().CreateCommand();

        var entitiesByType = entities
            .GroupBy(e => e.GetType())
            .Select(g => (EntityType: GetEntityType(dbContext, g.Key), Entities: g.ToArray()));

        var commands = new List<(string, DbParameter[])>();

        foreach (var (entityType, entitiesOfType) in entitiesByType)
        {
            var mapping = GetTableMapping(entityType);

            var columns = mapping.Columns.ToArray();
            var keyColumnNames = columns.Where(c => c.Property.IsPrimaryKey()).Select(c => c.ColumnName).ToArray();
            var nonKeyColumns = columns.Where(c => !c.Property.IsPrimaryKey()).ToArray();

            var rowsPerStatement = MaxParametersPerStatement / columns.Length;

            foreach (var chunk in entitiesOfType.Chunk(rowsPerStatement))
            {
                var parameters = new List<DbParameter>(chunk.Length * columns.Length);
                var sql = new StringBuilder();

                sql.Append("INSERT INTO ")
                    .Append(sqlGenerationHelper.DelimitIdentifier(mapping.Table.Name, mapping.Table.Schema))
                    .Append(" (")
                    .AppendJoin(", ", columns.Select(c => sqlGenerationHelper.DelimitIdentifier(c.ColumnName)))
                    .Append(") VALUES ");

                for (var row = 0; row < chunk.Length; row++)
                {
                    if (row > 0)
                    {
                        sql.Append(", ");
                    }

                    sql.Append('(');

                    for (var col = 0; col < columns.Length; col++)
                    {
                        if (col > 0)
                        {
                            sql.Append(", ");
                        }

                        var column = columns[col];
                        var parameterName = $"p{parameters.Count}";
                        var value = column.GetValue(chunk[row]);
                        var parameter = column.TypeMapping.CreateParameter(parameterFactory, parameterName, value, column.Property.IsNullable);
                        parameters.Add(parameter);

                        sql.Append(sqlGenerationHelper.GenerateParameterNamePlaceholder(parameterName));
                    }

                    sql.Append(')');
                }

                sql.Append(" ON CONFLICT (")
                    .AppendJoin(", ", keyColumnNames.Select(sqlGenerationHelper.DelimitIdentifier))
                    .Append(')');

                if (nonKeyColumns.Length == 0)
                {
                    sql.Append(" DO NOTHING");
                }
                else
                {
                    sql.Append(" DO UPDATE SET ")
                        .AppendJoin(", ", nonKeyColumns.Select(c =>
                        {
                            var name = sqlGenerationHelper.DelimitIdentifier(c.ColumnName);
                            return $"{name} = excluded.{name}";
                        }));
                }

                commands.Add((sql.ToString(), parameters.ToArray()));
            }
        }

        return commands;
    }

    private static IEntityType GetEntityType(DbContext dbContext, Type clrType)
    {
        var entityType = dbContext.Model.FindEntityType(clrType) ??
            throw new InvalidOperationException($"'{clrType.Name}' is not an entity type in {dbContext.GetType().Name}.");

        if (entityType.IsOwned())
        {
            throw new NotSupportedException($"'{entityType.DisplayName()}' is an owned type.");
        }

        if (entityType.IsMappedToJson())
        {
            throw new NotSupportedException($"'{entityType.DisplayName()}' is mapped to JSON.");
        }

        if (entityType.GetComplexProperties().Any())
        {
            throw new NotSupportedException($"'{entityType.DisplayName()}' has complex properties.");
        }

        if (entityType.GetNavigations().Any(n => n.TargetEntityType.IsOwned()))
        {
            throw new NotSupportedException($"'{entityType.DisplayName()}' has owned navigations.");
        }

        var mappingStrategy = entityType.GetMappingStrategy();
        if (mappingStrategy is RelationalAnnotationNames.TptMappingStrategy or RelationalAnnotationNames.TpcMappingStrategy)
        {
            throw new NotSupportedException($"'{entityType.DisplayName()}' uses the {mappingStrategy} mapping strategy.");
        }

        return entityType;
    }

    private static TableMapping GetTableMapping(IEntityType entityType)
    {
        var tableMappings = entityType.GetTableMappings().ToArray();

        if (tableMappings.Length != 1)
        {
            throw new NotSupportedException($"'{entityType.DisplayName()}' must be mapped to exactly one table.");
        }

        var table = tableMappings[0].Table;
        var storeObject = StoreObjectIdentifier.Table(table.Name, table.Schema);
        var discriminatorProperty = entityType.FindDiscriminatorProperty();

        var columns = entityType.GetProperties()
            .Where(p => p.GetComputedColumnSql(storeObject) is null)
            .Select(p =>
            {
                var columnName = p.GetColumnName(storeObject) ??
                    throw new NotSupportedException($"'{entityType.DisplayName()}.{p.Name}' is not mapped to a column.");

                Func<object, object?> getValue;

                if (p == discriminatorProperty)
                {
                    // Use the model's discriminator value rather than the CLR property, which EF populates on materialization only
                    var discriminatorValue = entityType.GetDiscriminatorValue();
                    getValue = _ => discriminatorValue;
                }
                else if (p.IsShadowProperty())
                {
                    throw new NotSupportedException($"'{entityType.DisplayName()}.{p.Name}' is a shadow property.");
                }
                else
                {
                    var getter = p.GetGetter();
                    getValue = getter.GetClrValueUsingContainingEntity;
                }

                return new ColumnMapping(p, columnName, p.GetRelationalTypeMapping(), getValue);
            })
            .ToArray();

        return new TableMapping(table, columns);
    }

    private sealed record TableMapping(ITable Table, ColumnMapping[] Columns);

    private sealed record ColumnMapping(IProperty Property, string ColumnName, RelationalTypeMapping TypeMapping, Func<object, object?> GetValue);
}
