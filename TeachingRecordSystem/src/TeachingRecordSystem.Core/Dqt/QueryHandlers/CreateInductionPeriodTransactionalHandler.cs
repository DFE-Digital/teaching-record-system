using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateInductionPeriodTransactionalHandler : ICrmTransactionalQueryHandler<CreateInductionPeriodTransactionalQuery, Guid>
{
    public Func<Guid> AppendQuery(CreateInductionPeriodTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = new dfeta_inductionperiod()
            {
                Id = query.Id,
                dfeta_InductionId = query.InductionId.ToEntityReference(dfeta_induction.EntityLogicalName),
                dfeta_AppropriateBodyId = query.AppropriateBodyId?.ToEntityReference(Account.EntityLogicalName),
                dfeta_StartDate = query.InductionStartDate,
                dfeta_EndDate = query.InductionEndDate,
            }
        });

        return () => createResponse.GetResponse().id;
    }
}
