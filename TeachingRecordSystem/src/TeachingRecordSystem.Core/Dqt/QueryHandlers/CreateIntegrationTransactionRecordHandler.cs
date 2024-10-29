using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateIntegrationTransactionRecordHandler : ICrmQueryHandler<CreateIntegrationTransactionRecordQuery, Guid>
{
    public async Task<Guid> Execute(CreateIntegrationTransactionRecordQuery query, IOrganizationServiceAsync organizationService)
    {
        var id = Guid.NewGuid();
        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);

        var integrationTransaction = new dfeta_integrationtransactionrecord()
        {
            Id = id,
            dfeta_IntegrationTransactionId = new EntityReference(dfeta_integrationtransaction.EntitySchemaName, query.IntegrationTransactionId),
            dfeta_id = query.Reference,
            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, query.PersonId),
            dfeta_InitialTeacherTrainingId = query.InitialTeacherTrainingId.HasValue ? new EntityReference(dfeta_initialteachertraining.EntityLogicalName, query.InitialTeacherTrainingId.Value) : null,
            dfeta_QualificationId = query.QualificationId.HasValue ? new EntityReference(dfeta_qualification.EntityLogicalName, query.QualificationId.Value) : null,
            dfeta_InductionId = query.InductionId.HasValue ? new EntityReference(dfeta_induction.EntityLogicalName, query.InductionId.Value) : null,
            dfeta_InductionPeriodId = query.InductionPeriodId.HasValue ? new EntityReference(dfeta_inductionperiod.EntityLogicalName, query.InductionPeriodId.Value) : null,
            //dfeta_DuplicateStatus = query.Duplicate == true ? new OptionSetValue((int)query.Duplicate)

        };
        requestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = integrationTransaction });
        await requestBuilder.Execute();

        return id;

        //if (duplicate)
        //    item.Attributes.Add(IntegrationTransactionRecord.Attributes.DuplicateStatus, new OptionSetValue((int)DuplicateStatus.Duplicate));
    }
}
