using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;

namespace TeachingRecordSystem.Core.Services.DqtReporting;

public class ReportingHelper(
    IOptions<DqtReportingOptions> optionsAccessor,
    IClock clock,
    IConfiguration configuration)
{
    public async Task BackfillTrsTableAsync(string tableName, CancellationToken cancellationToken)
    {
        await using var pgConnection = new NpgsqlConnection(configuration.GetPostgresConnectionString());
        await pgConnection.OpenAsync(cancellationToken);

        await using var sqlConnection = new SqlConnection(optionsAccessor.Value.ReportingDbConnectionString);
        await sqlConnection.OpenAsync(cancellationToken);

        // Get the column information for the specified table
        var columns = await GetTableColumnInfoAsync(pgConnection, tableName, cancellationToken);
        var pkColumns = await GetPrimaryKeyColumnsAsync(pgConnection, tableName, cancellationToken);

        const int BatchSize = 10000;

        var offset = 0;
        while (!cancellationToken.IsCancellationRequested)
        {
            var fetchSql = $"""
                SELECT {(string.Join(", ", columns.Select(c => $"{c.Name}")))}
                FROM {tableName}
                ORDER BY {string.Join(", ", pkColumns!)}
                LIMIT @limit OFFSET @offset
                """;

            await using var fetchCmd = new NpgsqlCommand(fetchSql, pgConnection);
            fetchCmd.Parameters.AddWithValue("limit", BatchSize);
            fetchCmd.Parameters.AddWithValue("offset", offset);

            var rows = new List<object?[]>();
            await using (var reader = await fetchCmd.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var values = new object?[columns.Count];
                    for (var i = 0; i < columns.Count; i++)
                    {
                        if (await reader.IsDBNullAsync(i, cancellationToken))
                        {
                            values[i] = null;
                            continue;
                        }

                        var raw = reader.GetValue(i);

                        // Note timestamp with time zone in Postgres maps to DateTime Kind.Utc in .NET
                        // So no need to handle separately
                        if (columns[i].DataType == "ARRAY" && raw is not null)
                        {
                            // Serialize Postgres arrays to JSON
                            values[i] = JsonSerializer.Serialize(raw);
                        }
                        else
                        {
                            values[i] = raw;
                        }
                    }

                    rows.Add(values);
                }
            }

            if (rows.Count == 0)
                break;

            var dataTable = new DataTable();
            foreach (var col in columns)
            {
                var colType = col.DataType == "ARRAY" ? typeof(string) : typeof(object);
                dataTable.Columns.Add(new DataColumn(col.Name, colType));
            }

            foreach (var row in rows)
            {
                var dr = dataTable.NewRow();
                for (var i = 0; i < columns.Count; i++)
                {
                    dr[i] = row[i] ?? DBNull.Value;
                }

                dataTable.Rows.Add(dr);
            }

            var tempName = $"#stg_{Guid.NewGuid():N}";
            var targetTableName = $"trs_{tableName}";

            // Create temp staging table with same schema as target
            var createTempSql = $"SELECT TOP (0) * INTO {tempName} FROM {targetTableName};";
            await using (var createCmd = new SqlCommand(createTempSql, sqlConnection))
            {
                createCmd.CommandTimeout = 60;
                await createCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            using (var bulk = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.TableLock, null))
            {
                bulk.DestinationTableName = tempName;
                bulk.BatchSize = Math.Max(1, rows.Count);
                bulk.BulkCopyTimeout = 60 * 5;

                foreach (var col in columns)
                {
                    bulk.ColumnMappings.Add(col.Name, col.Name);
                }

                await bulk.WriteToServerAsync(dataTable, cancellationToken);
            }

            var nowParameterName = "@UtcNow";

            var mergeSql = $"""
                MERGE {targetTableName} AS target
                USING {tempName} AS source
                ON ({string.Join(" AND ", pkColumns.Select(pk => $"target.[{pk}] = source.[{pk}]"))})
                WHEN NOT MATCHED THEN
                    INSERT ({(string.Join(", ", columns.Select(c => $"[{c.Name}]")))}, __Inserted, __Updated)
                    VALUES ({(string.Join(", ", columns.Select(c => $"source.[{c.Name}]")))}, {nowParameterName}, {nowParameterName})
                WHEN MATCHED THEN
                    UPDATE SET {(string.Join(", ", columns.Where(c => !pkColumns.Contains(c.Name)).Select(c => $"[{c.Name}] = source.[{c.Name}]")))}, __Updated = {nowParameterName}
                ;
                """;

            await using (var mergeCmd = new SqlCommand(mergeSql, sqlConnection))
            {
                mergeCmd.Parameters.Add(new SqlParameter(nowParameterName, clock.UtcNow));
                mergeCmd.CommandTimeout = 60 * 10;
                await mergeCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            // Drop staging table
            await using (var dropCmd = new SqlCommand($"DROP TABLE IF EXISTS {tempName};", sqlConnection))
            {
                await dropCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            offset += rows.Count;
        }
    }

    private record ColumnInfo(string Name, string DataType);

    private async Task<IReadOnlyList<ColumnInfo>> GetTableColumnInfoAsync(NpgsqlConnection connection, string table, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT column_name, data_type
            FROM information_schema.columns
            WHERE table_schema = 'public' AND table_name = @table
            ORDER BY ordinal_position;
        ";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("table", table);

        var cols = new List<ColumnInfo>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var columnName = reader.GetString(0);
            var dataType = reader.GetString(1);

            cols.Add(new ColumnInfo(columnName, dataType));
        }

        return cols;
    }

    private async Task<IReadOnlyList<string>> GetPrimaryKeyColumnsAsync(NpgsqlConnection connection, string table, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT kcu.column_name
            FROM information_schema.table_constraints tc
            JOIN information_schema.key_column_usage kcu
              ON tc.constraint_name = kcu.constraint_name
              AND tc.constraint_schema = kcu.constraint_schema
            WHERE tc.constraint_type = 'PRIMARY KEY'
              AND tc.table_schema = 'public'
              AND tc.table_name = @table
            ORDER BY kcu.ordinal_position;
        ";

        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("table", table);

        var pk = new List<string>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            pk.Add(reader.GetString(0));
        }

        return pk;
    }
}
