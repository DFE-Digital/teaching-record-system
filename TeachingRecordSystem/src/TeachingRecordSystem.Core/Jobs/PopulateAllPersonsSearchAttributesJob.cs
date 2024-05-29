using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

public class PopulateAllPersonsSearchAttributesJob(IDbContextFactory<TrsDbContext> dbContextFactory)
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        using var readDbContext = await dbContextFactory.CreateDbContextAsync();
        readDbContext.Database.SetCommandTimeout(TimeSpan.FromMinutes(15));

        var personIds = readDbContext.Database
            .SqlQuery<Guid>(
                $"""
                SELECT p.person_id FROM persons p
                LEFT JOIN (SELECT DISTINCT person_id FROM person_search_attributes) a ON p.person_id = a.person_id
                WHERE a.person_id IS NULL
                """)
            .AsAsyncEnumerable();

        await foreach (var chunk in personIds.Chunk(250).WithCancellation(cancellationToken))
        {
            using var writeDbContext = await dbContextFactory.CreateDbContextAsync();
            writeDbContext.Database.SetCommandTimeout(TimeSpan.FromSeconds(60));

            await writeDbContext.Database.ExecuteSqlRawAsync(
                "CALL p_refresh_person_search_attributes(:person_ids)",
                parameters: new object[]
                {
                    new NpgsqlParameter("person_ids", value: chunk)
                });
        }
    }
}

file static class Extensions
{
    public static async IAsyncEnumerable<T[]> Chunk<T>(this IAsyncEnumerable<T> source, int size)
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
