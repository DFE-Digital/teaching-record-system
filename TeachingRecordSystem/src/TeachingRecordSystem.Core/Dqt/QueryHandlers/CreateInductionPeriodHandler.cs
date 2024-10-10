using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateInductionPeriodHandler : ICrmTransactionalQueryHandler<CreateInductionPeriodQuery, Guid>
{
    public Func<Guid> AppendQuery(CreateInductionPeriodQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = new dfeta_inductionperiod()
            {
                dfeta_InductionId = query.Id.ToEntityReference(dfeta_induction.EntityLogicalName),
                dfeta_AppropriateBodyId = query.AppropriateBodyId?.ToEntityReference(Account.EntityLogicalName),
                dfeta_StartDate = query.InductionStartDate,
                dfeta_EndDate = query.InductionEndDate,
            }
        });

        return () => query.Id;
    }
}
