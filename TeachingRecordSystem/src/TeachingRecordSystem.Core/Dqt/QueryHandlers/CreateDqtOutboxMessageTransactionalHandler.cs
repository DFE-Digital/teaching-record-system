using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateDqtOutboxMessageTransactionalHandler : ICrmTransactionalQueryHandler<CreateDqtOutboxMessageTransactionalQuery, Guid>
{
    public Func<Guid> AppendQuery(CreateDqtOutboxMessageTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var serializer = new MessageSerializer();
        var message = serializer.CreateCrmOutboxMessage(query.Message);

        var createResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = message
        });

        return () => createResponse.GetResponse().id;
    }
}
