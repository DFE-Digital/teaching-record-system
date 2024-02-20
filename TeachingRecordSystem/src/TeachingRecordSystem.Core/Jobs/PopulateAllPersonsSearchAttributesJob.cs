using Hangfire;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class PopulateAllPersonsSearchAttributesJob(IDbContextFactory<TrsDbContext> dbContextFactory)
{
    public async Task Execute(CancellationToken cancellationToken)
    {
        await using var outerDbContext = await dbContextFactory.CreateDbContextAsync();
        await using var innerDbContext = await dbContextFactory.CreateDbContextAsync();

        await foreach (var person in outerDbContext.Persons.AsNoTracking().AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            await innerDbContext.Database.ExecuteSqlAsync(
                $"""
                CALL p_refresh_person_search_attributes(
                    {person.PersonId},
                    {person.FirstName},
                    {person.LastName},
                    {person.DateOfBirth},
                    {person.NationalInsuranceNumber},
                    {person.Trn})
                """);
        }
    }
}
