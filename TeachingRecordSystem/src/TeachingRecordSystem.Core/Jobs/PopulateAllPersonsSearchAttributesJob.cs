using Hangfire;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class PopulateAllPersonsSearchAttributesJob(TrsDbContext dbContext)
{
    public async Task Execute()
    {
        await foreach (var person in dbContext.Persons.AsNoTracking().AsAsyncEnumerable())
        {
            await dbContext.Database.ExecuteSqlAsync(
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
