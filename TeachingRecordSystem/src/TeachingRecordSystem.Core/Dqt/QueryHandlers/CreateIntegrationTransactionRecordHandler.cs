using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateIntegrationTransactionRecordHandler : ICrmTransactionalQueryHandler<CreateIntegrationTransactionRecordQuery, Guid>
{
    public Func<Guid> AppendQuery(CreateIntegrationTransactionRecordQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = new dfeta_integrationtransactionrecord()
            {
                Id = query.Id,
                dfeta_IntegrationTransactionId = query.IntegrationTransactionId.ToEntityReference(dfeta_integrationtransaction.EntitySchemaName),
                dfeta_id = query.Reference,
                dfeta_PersonId = query.PersonId?.ToEntityReference(Contact.EntityLogicalName),
                dfeta_InitialTeacherTrainingId = query.InitialTeacherTrainingId?.ToEntityReference(dfeta_initialteachertraining.EntityLogicalName),
                dfeta_QualificationId = query.QualificationId?.ToEntityReference(dfeta_qualification.EntityLogicalName),
                dfeta_InductionId = query.InductionId?.ToEntityReference(dfeta_induction.EntityLogicalName),
                dfeta_InductionPeriodId = query.InductionPeriodId?.ToEntityReference(dfeta_inductionperiod.EntityLogicalName),
                dfeta_DuplicateStatus = query.DuplicateStatus,
                dfeta_Filename = query.FileName
            }
        });

        return () => query.Id;
    }
}
