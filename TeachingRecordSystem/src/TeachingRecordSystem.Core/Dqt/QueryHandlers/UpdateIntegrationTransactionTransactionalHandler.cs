using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateIntegrationTransactionTransactionalHandler : ICrmTransactionalQueryHandler<UpdateIntegrationTransactionTransactionalQuery, bool>
{
    public Func<bool> AppendQuery(UpdateIntegrationTransactionTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var updateResponse = requestBuilder.AddRequest<UpdateResponse>(new UpdateRequest()
        {
            Target = new dfeta_integrationtransaction()
            {
                Id = query.IntegrationTransactionId,
                dfeta_EndDate = query.EndDate,
                dfeta_TotalCount = query.TotalCount,
                dfeta_SuccessCount = query.SuccessCount,
                dfeta_DuplicateCount = query.DuplicateCount,
                dfeta_FailureCount = query.FailureCount,
                dfeta_FailureMessage = query.FailureMessage,
            }
        });

        return () => true;
    }
}
