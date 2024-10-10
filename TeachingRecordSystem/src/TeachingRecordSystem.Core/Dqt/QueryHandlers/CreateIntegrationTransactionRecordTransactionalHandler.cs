using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateIntegrationTransactionRecordTransactionalHandler : ICrmTransactionalQueryHandler<CreateIntegrationTransactionRecordTransactionalQuery, Guid>
{
    public Func<Guid> AppendQuery(CreateIntegrationTransactionRecordTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = new dfeta_integrationtransactionrecord()
            {
                dfeta_IntegrationTransactionId = query.IntegrationTransactionId.ToEntityReference(dfeta_integrationtransaction.EntityLogicalName),
                dfeta_id = query.Reference,
                dfeta_PersonId = query.ContactId?.ToEntityReference(Contact.EntityLogicalName),
                dfeta_InitialTeacherTrainingId = query.InitialTeacherTrainingId?.ToEntityReference(dfeta_initialteachertraining.EntityLogicalName),
                dfeta_QualificationId = query.QualificationId?.ToEntityReference(dfeta_qualification.EntityLogicalName),
                dfeta_InductionId = query.InductionId?.ToEntityReference(dfeta_induction.EntityLogicalName),
                dfeta_InductionPeriodId = query.InductionPeriodId?.ToEntityReference(dfeta_inductionperiod.EntityLogicalName),
                dfeta_DuplicateStatus = query.DuplicateStatus,
                StatusCode = query.StatusCode,
                dfeta_FailureMessage = query.FailureMessage,
                dfeta_RowData = query.RowData,
                dfeta_Filename = query.FileName
            }
        });

        return () => createResponse.GetResponse().id;
    }
}
