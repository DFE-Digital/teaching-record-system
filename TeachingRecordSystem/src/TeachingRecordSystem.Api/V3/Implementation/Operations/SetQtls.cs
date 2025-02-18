using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record SetQtlsCommand(string Trn, DateOnly? QtsDate);

public class SetQtlsHandler(TrsDbContext dbContext, ICrmQueryDispatcher crmQueryDispatcher, IClock clock)
{
    public async Task<ApiResult<QtlsResult>> HandleAsync(SetQtlsCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_InductionStatus,
                    Contact.Fields.dfeta_qtlsdate,
                    Contact.Fields.dfeta_QTSDate)));

        if (contact is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        var hasActiveAlert = await dbContext.Alerts.Where(a => a.PersonId == contact.Id && a.IsOpen).AnyAsync();

        await crmQueryDispatcher.ExecuteQueryAsync(
             new SetQtlsDateQuery(contact.Id, command.QtsDate, hasActiveAlert, clock.UtcNow));

        return new QtlsResult()
        {
            Trn = command.Trn,
            QtsDate = command.QtsDate
        };
    }
}

