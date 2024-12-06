using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetDeceasedCommand(string Trn, DateOnly DateOfDeath);

public class SetDeceasedHandler(ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task<ApiResult<Unit>> HandleAsync(SetDeceasedCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet()));

        if (contact is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        await crmQueryDispatcher.ExecuteQueryAsync(new SetDeceasedQuery(contact.Id, command.DateOfDeath))!;

        return Unit.Instance;
    }
}
