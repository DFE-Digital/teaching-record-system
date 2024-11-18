using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class SetDeceasedHandler : ICrmQueryHandler<SetDeceasedQuery, bool>
{
    public async Task<bool> ExecuteAsync(SetDeceasedQuery query, IOrganizationServiceAsync organizationService)
    {
        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);
        requestBuilder.AddRequest(new UpdateRequest()
        {
            Target = new Contact()
            {
                Id = query.ContactId,
                dfeta_DateofDeath = query.DateOfDeath.ToDateTimeWithDqtBstFix(isLocalTime: false)
            }
        });
        await requestBuilder.ExecuteAsync();

        return true;
    }
}
