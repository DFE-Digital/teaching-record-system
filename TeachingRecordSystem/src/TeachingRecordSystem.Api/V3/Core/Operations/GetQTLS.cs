using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;

public record GetQTLSCommand(string Trn);

public class GetQTLSHandler(ICrmQueryDispatcher _crmQueryDispatcher)
{
    public async Task<QTLSInfo?> Handle(GetQTLSCommand command)
    {
        var contact = (await _crmQueryDispatcher.ExecuteQuery(
            new GetContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN//
                    //Contact.Fields.QTLS_Date
                    )
                )
            ))!;

        if(contact is null)
        {
            return null;
        }

        return new QTLSInfo()
        {
            Trn = command.Trn,
            //AwardedDate = contact.QTLS_Date
            AwardedDate = null
        };
    }
}
