using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateTaskTransactionalHandler : ICrmTransactionalQueryHandler<CreateTaskTransactionalQuery, Guid>
{
    public Func<Guid> AppendQuery(CreateTaskTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = new CrmTask()
            {
                RegardingObjectId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
                Category = query.Category,
                Subject = query.Subject,
                Description = query.Description,
                ScheduledEnd = query.ScheduledEnd
            }
        });

        return () => createResponse.GetResponse().id;
    }
}
