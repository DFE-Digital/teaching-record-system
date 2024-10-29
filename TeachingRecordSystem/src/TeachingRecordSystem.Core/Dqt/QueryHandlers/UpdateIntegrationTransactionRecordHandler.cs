using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class UpdateIntegrationTransactionRecordHandler : ICrmQueryHandler<UpdateIntegrationTransactionRecordQuery, bool>
{
    public async Task<bool> Execute(UpdateIntegrationTransactionRecordQuery query, IOrganizationServiceAsync organizationService)
    {
        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);
        var integrationTransaction = new dfeta_integrationtransactionrecord()
        {
            Id = query.IntegrationTransactionRecordId,
            dfeta_IntegrationTransactionId = new EntityReference(dfeta_integrationtransaction.EntitySchemaName, query.IntegrationTransactionId),
            dfeta_id = query.Reference,
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, query.PersonId),
            dfeta_InitialTeacherTrainingId = query.InitialTeacherTrainingId.HasValue ? new EntityReference(dfeta_initialteachertraining.EntityLogicalName, query.InitialTeacherTrainingId.Value) : null,
            dfeta_QualificationId = query.QualificationId.HasValue ? new EntityReference(dfeta_qualification.EntityLogicalName, query.QualificationId.Value) : null,
            dfeta_InductionId = query.InductionId.HasValue ? new EntityReference(dfeta_induction.EntityLogicalName, query.InductionId.Value) : null,
            dfeta_InductionPeriodId = query.InductionPeriodId.HasValue ? new EntityReference(dfeta_inductionperiod.EntityLogicalName, query.InductionPeriodId.Value) : null,
            dfeta_DuplicateStatus = query.Duplicate == true ? dfeta_integrationtransactionrecord_dfeta_DuplicateStatus.Duplicate : null
        };
        requestBuilder.AddRequest<UpdateResponse>(new UpdateRequest() { Target = integrationTransaction });
        await requestBuilder.Execute();

        return true;
    }
}
