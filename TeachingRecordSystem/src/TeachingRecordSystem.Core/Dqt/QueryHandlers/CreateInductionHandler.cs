using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateInductionHandler : ICrmTransactionalQueryHandler<CreateInductionQuery, Guid>
{
    public Func<Guid> AppendQuery(CreateInductionQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = new dfeta_induction()
            {
                Id = query.Id,
                dfeta_PersonId = query.PersonId?.ToEntityReference(Contact.EntityLogicalName),
                dfeta_StartDate = query.StartDate,
                dfeta_CompletionDate = query.CompletionDate,
                dfeta_InductionStatus = query.InductionStatus,
            }
        });
        return () => query.Id;
    }
}
