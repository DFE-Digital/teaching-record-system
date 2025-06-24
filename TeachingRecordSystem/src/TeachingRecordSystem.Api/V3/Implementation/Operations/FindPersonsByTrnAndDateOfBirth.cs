using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record FindPersonsByTrnAndDateOfBirthCommand(IEnumerable<(string Trn, DateOnly DateOfBirth)> Persons);

public class FindPersonsByTrnAndDateOfBirthHandler(
    TrsDbContext dbContext,
    ICrmQueryDispatcher crmQueryDispatcher,
    PreviousNameHelper previousNameHelper,
    ReferenceDataCache referenceDataCache,
    IFeatureProvider featureProvider) :
    FindPersonsHandlerBase(dbContext, crmQueryDispatcher, previousNameHelper, referenceDataCache, featureProvider)
{
    public async Task<ApiResult<FindPersonsResult>> HandleAsync(FindPersonsByTrnAndDateOfBirthCommand command)
    {
        var contacts = await CrmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactsByTrnsQuery(
                command.Persons.Select(p => p.Trn).Where(trn => !string.IsNullOrEmpty(trn)).Distinct(),
                ContactColumnSet));

        // Remove any results where the request DOB doesn't match the contact's DOB
        // (we can't easily do this in the query itself).
        var matched = contacts
            .Where(kvp => kvp.Value is not null)
            .Where(kvp => command.Persons.First(p => p.Trn == kvp.Key).DateOfBirth == kvp.Value!.BirthDate?.ToDateOnlyWithDqtBstFix(isLocalTime: false))
            .Select(kvp => kvp.Value!)
            .ToArray();

        return await CreateResultAsync(matched);
    }
}
