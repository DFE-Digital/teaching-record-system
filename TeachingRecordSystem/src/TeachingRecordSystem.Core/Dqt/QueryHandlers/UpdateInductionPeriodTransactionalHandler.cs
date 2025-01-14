using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateInductionPeriodTransactionalHandler : ICrmTransactionalQueryHandler<UpdateInductionPeriodTransactionalQuery, bool>
{
    public Func<bool> AppendQuery(UpdateInductionPeriodTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var updateResponse = requestBuilder.AddRequest<UpdateResponse>(new UpdateRequest()
        {
            Target = new dfeta_inductionperiod()
            {
                Id = query.InductionPeriodId,
                dfeta_AppropriateBodyId = query.AppropriateBodyId?.ToEntityReference(Account.EntityLogicalName),
                dfeta_StartDate = query.InductionStartDate,
                dfeta_EndDate = query.InductionEndDate,
            }
        });

        return () => true;
    }
}
