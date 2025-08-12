using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record FindPersonByLastNameAndDateOfBirthCommand(string LastName, DateOnly DateOfBirth);

public class FindPersonByLastNameAndDateOfBirthHandler(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache) :
    FindPersonsHandlerBase(dbContext, referenceDataCache)
{
    public async Task<ApiResult<FindPersonsResult>> HandleAsync(FindPersonByLastNameAndDateOfBirthCommand command)
    {
        var matchedPersons = await DbContext.Database.SqlQueryRaw<Result>(
                """
                SELECT DISTINCT p.person_id FROM persons p
                LEFT JOIN previous_names pn ON p.person_id = pn.person_id
                WHERE p.date_of_birth = :date_of_birth
                AND (p.last_name = :last_name OR pn.last_name = :last_name)
                AND p.status = 0
                """,
                parameters: [
                    new NpgsqlParameter("last_name", command.LastName),
                    new NpgsqlParameter("date_of_birth", command.DateOfBirth)
                ]
            ).ToArrayAsync();

        return await CreateResultAsync(matchedPersons.Select(r => r.person_id).Distinct().AsReadOnly());
    }

#pragma warning disable IDE1006 // Naming Styles
    private record Result(Guid person_id);
#pragma warning restore IDE1006 // Naming Styles
}
