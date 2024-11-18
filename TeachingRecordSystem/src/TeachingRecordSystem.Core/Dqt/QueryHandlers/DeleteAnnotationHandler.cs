using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class DeleteAnnotationHandler : ICrmQueryHandler<DeleteAnnotationQuery, bool>
{
    public async Task<bool> ExecuteAsync(DeleteAnnotationQuery query, IOrganizationServiceAsync organizationService)
    {
        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);
        requestBuilder.AddRequest(new DeleteRequest() { Target = new(Annotation.EntityLogicalName, query.AnnotationId) });
        requestBuilder.AddRequest(new CreateRequest()
        {
            Target = new dfeta_TRSEvent()
            {
                dfeta_TRSEventId = query.Event.Event.EventId,
                dfeta_EventName = query.Event.Event.GetEventName(),
                dfeta_Payload = query.Event.Serialize()
            }
        });

        await requestBuilder.ExecuteAsync();

        return true;
    }
}
