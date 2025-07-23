using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record FindPersonsByTrnAndDateOfBirthCommand(IEnumerable<(string Trn, DateOnly DateOfBirth)> Persons);

public class FindPersonsByTrnAndDateOfBirthHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    PreviousNameHelper previousNameHelper,
    ReferenceDataCache referenceDataCache) :
    FindPersonsHandlerBase(dbContext, crmQueryDispatcher, previousNameHelper, referenceDataCache)
{
    public async Task<ApiResult<FindPersonsResult>> HandleAsync(FindPersonsByTrnAndDateOfBirthCommand command)
    {
        var trns = command.Persons.Select(t => t.Trn).ToArray();
        var persons = await DbContext.Persons
            .Where(p => trns.Contains(p.Trn))
            .Select(p => new { p.PersonId, p.DateOfBirth, p.Trn })
            .ToArrayAsync();

        // Remove any results where the request DOB doesn't match the contact's DOB
        // (we can't easily do this in the query itself).
        var matched = persons
            .Where(t => command.Persons.First(p => p.Trn == t.Trn).DateOfBirth == t.DateOfBirth)
            .Select(t => t.PersonId)
            .ToArray();

        return await CreateResultAsync(matched);
    }
}
