using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record GetQtlsCommand(string Trn);

public class GetQtlsHandler(ICrmQueryDispatcher _crmQueryDispatcher)
{
    public async Task<QtlsInfo?> Handle(GetQtlsCommand command)
    {
        var contact = (await _crmQueryDispatcher.ExecuteQuery(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_qtlsdate
                    )
                )
            ))!;

        if (contact is null)
        {
            return null;
        }

        return new QtlsInfo()
        {
            Trn = command.Trn,
            QtsDate = contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
        };
    }
}
