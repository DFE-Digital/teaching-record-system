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
                SELECT person_id FROM (
                    SELECT person_id, array_agg(attribute_type) matched_attr_keys
                    FROM person_search_attributes
                    WHERE (attribute_type in ('LastName', 'PreviousLastName') AND attribute_value = :last_name)
                       OR (attribute_type = 'DateOfBirth' AND attribute_value = :date_of_birth)
                    GROUP BY person_id ) x
                WHERE 'DateOfBirth' = ANY(matched_attr_keys) AND ARRAY['LastName', 'PreviousLastName']::varchar[] && matched_attr_keys
                """,
                parameters: [
                    new NpgsqlParameter("last_name", command.LastName),
                    new NpgsqlParameter("date_of_birth", command.DateOfBirth.ToString("yyyy-MM-dd"))
                ]
            ).ToArrayAsync();

        return await CreateResultAsync(matchedPersons.Select(r => r.person_id).Distinct().AsReadOnly());
    }

#pragma warning disable IDE1006 // Naming Styles
    private record Result(Guid person_id);
#pragma warning restore IDE1006 // Naming Styles
}
