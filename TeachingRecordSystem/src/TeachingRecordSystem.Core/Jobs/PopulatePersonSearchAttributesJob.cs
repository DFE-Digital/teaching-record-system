using Hangfire;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Core.Jobs;

[AutomaticRetry(Attempts = 0)]
public class PopulatePersonSearchAttributesJob(TrsDbContext dbContext)
{
    public async Task Execute(Guid personId)
    {
        var person = await dbContext.Persons.AsNoTracking().SingleAsync(p => p.PersonId == personId);
        _ = await dbContext.Database.ExecuteSqlAsync(
            $"""
            CALL p_refresh_person_search_attributes(
                {personId},
                {person.FirstName},
                {person.LastName},
                {person.DateOfBirth},
                {person.NationalInsuranceNumber},
                {person.Trn})
            """);
    }
}
