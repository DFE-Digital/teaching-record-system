using System.Reflection.Metadata.Ecma335;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Core.SharedModels;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Migrations;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Core.Operations;


public record SetQTLSCommand(string Trn, DateOnly? AwardedDate);

public class SetQTLSHandler(ICrmQueryDispatcher _crmQueryDispatcher)
{
    public async Task<QTLSInfo?> Handle(SetQTLSCommand command)
    {

        var contact = (await _crmQueryDispatcher.ExecuteQuery(
            new GetContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN
                    )
                )
            ))!;


        if(contact == null)
        {
            return null;
        }

       await _crmQueryDispatcher.ExecuteQuery(
            new SetQTLSDateQuery(contact.Id, command.AwardedDate))!;


        contact = (await _crmQueryDispatcher.ExecuteQuery(
            new GetContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN//,
                    //Contact.Fields.dfeta_qtls_date
                    )
                )
            ))!;

        return new QTLSInfo()
        {
            Trn = command.Trn,
            AwardedDate = null
            //AwardedDate = contact.QTLS_Date
        };
    }
}

