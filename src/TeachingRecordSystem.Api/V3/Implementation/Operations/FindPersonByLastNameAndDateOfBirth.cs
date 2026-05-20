using Npgsql;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record FindPersonByLastNameAndDateOfBirthCommand(string LastName, DateOnly DateOfBirth) : ICommand<FindPersonsResult>;

public class FindPersonByLastNameAndDateOfBirthHandler(
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache) :
    FindPersonsHandlerBase(dbContext, referenceDataCache),
    ICommandHandler<FindPersonByLastNameAndDateOfBirthCommand, FindPersonsResult>
{
    public async Task<ApiResult<FindPersonsResult>> ExecuteAsync(FindPersonByLastNameAndDateOfBirthCommand command)
    {
        var matchedPersons = await DbContext.Database.SqlQueryRaw<Result>(
                """
                SELECT person_id FROM persons
                WHERE last_names && fn_split_names(ARRAY[:last_names]::varchar[]) COLLATE "case_insensitive" AND
                date_of_birth = :date_of_birth
                """,
                // ReSharper disable FormatStringProblem
                parameters: [
                    new NpgsqlParameter("last_names", command.LastName),
                    new NpgsqlParameter("date_of_birth", command.DateOfBirth)
                ]
                // ReSharper restore FormatStringProblem
            ).ToArrayAsync();

        return await CreateResultAsync(matchedPersons.Select(r => r.person_id).Distinct().AsReadOnly());
    }

#pragma warning disable IDE1006 // Naming Styles
    private record Result(Guid person_id);
#pragma warning restore IDE1006 // Naming Styles
}
