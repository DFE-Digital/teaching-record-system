namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetTrnCommand(string Trn);

public record GetTrnResult;

public class GetTrnHandler(GetPersonHelper getPersonHelper)
{
    public async Task<ApiResult<GetTrnResult>> HandleAsync(GetTrnCommand command)
    {
        var getPersonResult = await getPersonHelper.GetPersonByTrnAsync(command.Trn);

        return getPersonResult.Match<ApiResult<GetTrnResult>>(
            error => error,
            _ => new GetTrnResult());
    }
}

