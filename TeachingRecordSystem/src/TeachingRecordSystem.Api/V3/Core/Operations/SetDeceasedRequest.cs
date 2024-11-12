using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record SetDeceasedCommand(string Trn, DateOnly DateOfDeath);

public class SetDeceasedHandler(ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task Handle(SetDeceasedCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQuery(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet()));

        if (contact == null)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }

        await crmQueryDispatcher.ExecuteQuery(new SetDeceasedQuery(contact.Id, command.DateOfDeath))!;
    }
}
