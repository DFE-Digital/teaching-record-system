using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

public class PopulateAllPersonsSearchAttributesJob(IDbContextFactory<TrsDbContext> dbContextFactory)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var readDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        readDbContext.Database.SetCommandTimeout(0);

        var personIds = readDbContext.Database
            .SqlQuery<Guid>(
                $"""
                SELECT p.person_id FROM persons p
                LEFT JOIN (SELECT DISTINCT person_id FROM person_search_attributes) a ON p.person_id = a.person_id
                WHERE a.person_id IS NULL
                """)
            .AsAsyncEnumerable();

        await foreach (var chunk in personIds.ChunkAsync(250).WithCancellation(cancellationToken))
        {
            using var writeDbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            writeDbContext.Database.SetCommandTimeout(0);

            await writeDbContext.Database.ExecuteSqlRawAsync(
                "CALL p_refresh_person_search_attributes(:person_ids)",
                parameters:
                [
                    new NpgsqlParameter("person_ids", value: chunk)
                ]);

            await writeDbContext.Database.ExecuteSqlRawAsync(
                "CALL p_refresh_previous_names_person_search_attributes(:person_ids)",
                parameters:
                [
                    new NpgsqlParameter("person_ids", value: chunk)
                ]);
        }
    }
}

file static class Extensions
{
    public static async IAsyncEnumerable<T[]> ChunkAsync<T>(this IAsyncEnumerable<T> source, int size)
    {
        var buffer = new List<T>(size);

        await foreach (var item in source)
        {
            buffer.Add(item);

            if (buffer.Count == size)
            {
                yield return buffer.ToArray();
                buffer.Clear();
            }
        }

        if (buffer.Count > 0)
        {
            yield return buffer.ToArray();
        }
    }
}
