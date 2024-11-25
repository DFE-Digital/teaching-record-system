using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetDeceasedCommand(string Trn, DateOnly DateOfDeath);

public class SetDeceasedHandler(ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task HandleAsync(SetDeceasedCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet()));

        if (contact == null)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }

        await crmQueryDispatcher.ExecuteQueryAsync(new SetDeceasedQuery(contact.Id, command.DateOfDeath))!;
    }
}
