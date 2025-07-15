using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record FindPersonByLastNameAndDateOfBirthCommand(string LastName, DateOnly DateOfBirth);

public class FindPersonByLastNameAndDateOfBirthHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    PreviousNameHelper previousNameHelper,
    ReferenceDataCache referenceDataCache) :
    FindPersonsHandlerBase(dbContext, crmQueryDispatcher, previousNameHelper, referenceDataCache)
{
    public async Task<ApiResult<FindPersonsResult>> HandleAsync(FindPersonByLastNameAndDateOfBirthCommand command)
    {
        var matched = await CrmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactsByLastNameAndDateOfBirthQuery(
                command.LastName,
                command.DateOfBirth,
                ContactColumnSet));

        return await CreateResultAsync(matched);
    }
}
