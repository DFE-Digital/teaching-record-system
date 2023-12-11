using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateSanctionHandler : ICrmQueryHandler<CreateSanctionQuery, Guid>
{
    public async Task<Guid> Execute(CreateSanctionQuery query, IOrganizationServiceAsync organizationService)
    {
        var sanction = new dfeta_sanction()
        {
            Id = Guid.NewGuid(),
            dfeta_PersonId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_SanctionCodeId = query.SanctionCodeId.ToEntityReference(dfeta_sanctioncode.EntityLogicalName),
            dfeta_SanctionDetails = query.Details,
            dfeta_DetailsLink = query.Link,
            dfeta_StartDate = query.StartDate.ToDateTimeWithDqtBstFix(isLocalTime: true)
        };

        var sanctionId = await organizationService.CreateAsync(sanction);
        return sanctionId;
    }
}
