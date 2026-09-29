using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

public class PersonUnaccentNamesJob(TrsDbContext dbContext)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        dbContext.Database.SetCommandTimeout(0);

        const string sql =
            """
            WITH batch AS (
                SELECT person_id
                FROM persons
                WHERE first_name COLLATE "C" <> unaccent(first_name)
                    OR middle_name COLLATE "C" <> unaccent(middle_name)
                    OR last_name COLLATE "C" <> unaccent(last_name)
                ORDER BY person_id
                LIMIT 100
            )
            UPDATE persons p
            SET first_name = unaccent(p.first_name),
                middle_name = unaccent(p.middle_name),
                last_name = unaccent(p.last_name)
            FROM batch
            WHERE p.person_id = batch.person_id
            """;

        int updated;
        do
        {
            updated = await dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
        }
        while (updated > 0);
    }
}
