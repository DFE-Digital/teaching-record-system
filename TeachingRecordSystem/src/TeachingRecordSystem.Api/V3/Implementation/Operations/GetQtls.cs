using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public record GetQtlsCommand(string Trn);

public class GetQtlsHandler(ICrmQueryDispatcher crmQueryDispatcher)
{
    public async Task<ApiResult<QtlsResult>> HandleAsync(GetQtlsCommand command)
    {
        var contact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_qtlsdate)));

        if (contact is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        return new QtlsResult()
        {
            Trn = command.Trn,
            QtsDate = contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
        };
    }
}
