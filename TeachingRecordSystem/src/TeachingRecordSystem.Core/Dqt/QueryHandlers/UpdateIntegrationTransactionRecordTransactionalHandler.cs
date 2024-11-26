using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateIntegrationTransactionRecordTransactionalHandler : ICrmTransactionalQueryHandler<UpdateIntegrationTransactionRecordTransactionalQuery, bool>
{
    public Func<bool> AppendQuery(UpdateIntegrationTransactionRecordTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<UpdateResponse>(new UpdateRequest()
        {
            Target = new dfeta_integrationtransactionrecord()
            {
                Id = query.IntegrationTransactionRecordId,
                dfeta_IntegrationTransactionId = query.IntegrationTransactionId.ToEntityReference(dfeta_integrationtransaction.EntityLogicalName),
                dfeta_id = query.Reference,
                dfeta_PersonId = query.PersonId?.ToEntityReference(Contact.EntityLogicalName),
                dfeta_InitialTeacherTrainingId = query.InitialTeacherTrainingId?.ToEntityReference(dfeta_initialteachertraining.EntityLogicalName),
                dfeta_QualificationId = query.QualificationId?.ToEntityReference(dfeta_qualification.EntityLogicalName),
                dfeta_InductionId = query.InductionId?.ToEntityReference(dfeta_induction.EntityLogicalName),
                dfeta_InductionPeriodId = query.InductionPeriodId?.ToEntityReference(dfeta_inductionperiod.EntityLogicalName),
                dfeta_DuplicateStatus = query.DuplicateStatus,
                StatusCode = query.StatusCode,
                dfeta_FailureMessage = query.FailureMessage,
                dfeta_RowData = query.RowData
            }
        });

        return () => true;
    }
}
