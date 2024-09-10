using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Validation;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record SetDeceasedCommand(string TRN, DateOnly DateOfDeath);

public class SetDeceasedHandler(ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task Handle(SetDeceasedCommand command)
    {
        var contact = (await crmQueryDispatcher.ExecuteQuery(
            new GetActiveContactByTrnQuery(
                command.TRN,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_qtlsdate,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_ActiveSanctions))
            ))!;

        if (contact == null)
        {
            throw new ErrorException(ErrorRegistry.TeacherWithSpecifiedTrnNotFound());
        }

        await crmQueryDispatcher.ExecuteQuery(new SetDeceasedQuery(contact.Id, command.DateOfDeath))!;
    }
}
