using System.Diagnostics;
using OneOf;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public class GetPersonHelper(TrsDbContext dbContext)
{
    public async Task<OneOf<ApiError, PostgresModels.Person>> GetPersonByTrnAsync(
        string trn,
        Func<IQueryable<PostgresModels.Person>, IQueryable<PostgresModels.Person>>? configureQuery = null)
    {
        var query = dbContext.Persons
            .FromSql(
                $"""
                 with recursive active_persons(person_id) as (
                     select person_id from persons where trn = {trn}
                     union all
                     select persons.merged_with_person_id from persons, active_persons
                     where persons.person_id = active_persons.person_id
                 )
                 select p.* from persons p
                 join active_persons a on p.person_id = a.person_id
                 where p.merged_with_person_id is null
                 """)
            .IgnoreQueryFilters();

        if (configureQuery is not null)
        {
            query = configureQuery(query);
        }

        var person = await query.SingleOrDefaultAsync();

        if (person is null)
        {
            return ApiError.PersonNotFound(trn);
        }

        if (person.Status is PersonStatus.Inactive)
        {
            return ApiError.RecordIsNotActive(trn);
        }

        Debug.Assert(person.Trn is not null);

        if (person.Trn != trn)
        {
            return ApiError.RecordIsMerged(trn, person.Trn!);
        }

        return person;
    }
}
