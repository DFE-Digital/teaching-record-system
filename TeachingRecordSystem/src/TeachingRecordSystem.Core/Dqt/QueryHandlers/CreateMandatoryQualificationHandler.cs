using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateMandatoryQualificationHandler : ICrmQueryHandler<CreateMandatoryQualificationQuery, Guid>
{
    public async Task<Guid> Execute(CreateMandatoryQualificationQuery query, IOrganizationServiceAsync organizationService)
    {
        var qualification = new dfeta_qualification()
        {
            Id = Guid.NewGuid(),
            dfeta_Type = dfeta_qualification_dfeta_Type.MandatoryQualification,
            dfeta_name = "Mandatory Qualification",
            dfeta_PersonId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_MQ_MQEstablishmentId = query.MqEstablishmentId.ToEntityReference(dfeta_mqestablishment.EntityLogicalName),
            dfeta_MQ_SpecialismId = query.SpecialismId.ToEntityReference(dfeta_specialism.EntityLogicalName),
            dfeta_MQStartDate = query.StartDate.ToDateTimeWithDqtBstFix(isLocalTime: true),
            dfeta_MQ_Status = query.Status,
            dfeta_MQ_Date = query.EndDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
        };

        var qualificationId = await organizationService.CreateAsync(qualification);
        return qualificationId;
    }
}
