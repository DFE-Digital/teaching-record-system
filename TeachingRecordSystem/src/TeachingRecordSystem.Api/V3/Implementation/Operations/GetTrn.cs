namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetTrnCommand(string Trn) : ICommand<GetTrnResult>;

public record GetTrnResult;

public class GetTrnHandler(GetPersonHelper getPersonHelper) : ICommandHandler<GetTrnCommand, GetTrnResult>
{
    public async Task<ApiResult<GetTrnResult>> ExecuteAsync(GetTrnCommand command)
    {
        var getPersonResult = await getPersonHelper.GetPersonByTrnAsync(command.Trn);

        return getPersonResult.Match<ApiResult<GetTrnResult>>(
            error => error,
            _ => new GetTrnResult());
    }
}

