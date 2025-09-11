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
                SELECT person_id FROM person_search_attributes
                WHERE (attribute_type = 'LastName' AND attribute_value IN (:last_names COLLATE "case_insensitive")) OR
                      (attribute_type = 'DateOfBirth' AND attribute_value = (:date_of_birth COLLATE "case_insensitive"))
                GROUP BY person_id
                HAVING COUNT(DISTINCT attribute_type) = 2
                """,
                parameters: [
                    new NpgsqlParameter("last_names", PersonSearchAttribute.SplitNames(command.LastName)),
                    new NpgsqlParameter("date_of_birth", command.DateOfBirth.ToString("yyyy-MM-dd"))
                ]
            ).ToArrayAsync();

        return await CreateResultAsync(matchedPersons.Select(r => r.person_id).Distinct().AsReadOnly());
    }

#pragma warning disable IDE1006 // Naming Styles
    private record Result(Guid person_id);
#pragma warning restore IDE1006 // Naming Styles
}
