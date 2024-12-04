using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateInductionTransactionalHandler : ICrmTransactionalQueryHandler<UpdateInductionTransactionalQuery, bool>
{
    public Func<bool> AppendQuery(UpdateInductionTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<UpdateResponse>(new UpdateRequest()
        {
            Target = new dfeta_induction()
            {
                Id = query.InductionId,
                dfeta_CompletionDate = query.CompletionDate,
                dfeta_InductionStatus = query.InductionStatus,
            }
        });

        return () => true;
    }
}
