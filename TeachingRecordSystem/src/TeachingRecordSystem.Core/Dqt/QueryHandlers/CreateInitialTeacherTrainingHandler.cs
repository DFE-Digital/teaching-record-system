using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateInitialTeacherTrainingHandler : ICrmQueryHandler<CreateInitialTeacherTrainingQuery, Guid>
{
    public async Task<Guid> Execute(CreateInitialTeacherTrainingQuery query, IOrganizationServiceAsync organizationService)
    {
        var IttId = await organizationService.CreateAsync(new dfeta_initialteachertraining()
        {
            dfeta_PersonId = query.PersonId.Value.ToEntityReference(Contact.EntityLogicalName),
            dfeta_CountryId = query.CountryId.Value.ToEntityReference(dfeta_country.EntityLogicalName),
            dfeta_ITTQualificationId = query.ITTQualificationId.Value.ToEntityReference(dfeta_qualification.EntityLogicalName),
            dfeta_Result = query.Result
        });

        return IttId;
    }
}

