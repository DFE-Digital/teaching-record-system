using OneOf;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public class GetPersonHelper(TrsDbContext dbContext)
{
    public async Task<OneOf<ApiError, (Guid PersonId, string Trn)>> GetPersonByTrnAsync(string trn)
    {
        var result = await dbContext.Database
            .SqlQuery<Result>($"select * from fn_resolve_record_by_trn({trn})")
            .SingleOrDefaultAsync();

        if (result is null)
        {
            return ApiError.PersonNotFound(trn);
        }

        if (result.Status is PersonStatus.Deactivated)
        {
            return ApiError.RecordIsDeactivated(trn);
        }

        if (result.Trn != trn)
        {
            return ApiError.RecordIsMerged(trn, result.Trn);
        }

        return (result.PersonId, result.Trn);
    }

    [UsedImplicitly]
    private record Result(Guid PersonId, string Trn, PersonStatus Status);
}
