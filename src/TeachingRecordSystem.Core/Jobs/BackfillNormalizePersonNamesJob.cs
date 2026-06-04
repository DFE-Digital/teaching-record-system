using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

public class BackfillNormalizePersonNamesJob(TrsDbContext dbContext)
{
    public async Task ExecuteAsync(bool dryRun, CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Find all person_ids where names or last_names contain non-normalized quote characters:
        // U+0060 (grave accent), U+00B4 (acute accent), U+2018 (left single quote), U+2019 (right single quote)
        var personIdsToUpdateSql =
            """
            SELECT person_id
            FROM persons p
            WHERE EXISTS (
                SELECT 1
                FROM unnest(p.names) AS name
                WHERE name COLLATE "C" ~ '[\u0060\u00B4\u2018\u2019]'
            )
            OR EXISTS (
                SELECT 1
                FROM unnest(p.last_names) AS last_name
                WHERE last_name COLLATE "C" ~ '[\u0060\u00B4\u2018\u2019]'
            )
            """;

        var personIds = await dbContext.Database
            .SqlQueryRaw<Guid>(personIdsToUpdateSql)
            .ToArrayAsync(cancellationToken);

        if (personIds.Length == 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return;
        }

        // Call p_refresh_person_names to update the names using fn_split_names (which now normalizes quotes)
        var callProcedureSql = "CALL p_refresh_person_names(@p_person_ids)";
        await dbContext.Database.ExecuteSqlRawAsync(
            callProcedureSql,
            [new Npgsql.NpgsqlParameter("@p_person_ids", personIds)],
            cancellationToken);

        if (dryRun)
        {
            await transaction.RollbackAsync(cancellationToken);
        }
        else
        {
            await transaction.CommitAsync(cancellationToken);
        }
    }
}
