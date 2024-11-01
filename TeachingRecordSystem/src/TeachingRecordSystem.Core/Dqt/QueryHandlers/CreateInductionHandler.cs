using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateInductionHandler : ICrmQueryHandler<CreateInductionQuery, Guid>
{
    public async Task<Guid> Execute(CreateInductionQuery query, IOrganizationServiceAsync organizationService)
    {
        var inductionId = await organizationService.CreateAsync(new dfeta_induction()
        {
            dfeta_PersonId = query.PersonId!.Value.ToEntityReference(Contact.EntityLogicalName),
            dfeta_StartDate = query.StartDate,
            dfeta_CompletionDate = query.CompletionDate,
            dfeta_InductionStatus = query.InductionStatus,
        });

        return inductionId;
    }
}
