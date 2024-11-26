using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateQtsRegistrationTransactionalHandler : ICrmTransactionalQueryHandler<CreateQtsRegistrationTransactionalQuery, Guid>
{
    public Func<Guid> AppendQuery(CreateQtsRegistrationTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = new dfeta_qtsregistration()
            {
                Id = query.Id,
                dfeta_PersonId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_TeacherStatusId = query.TeacherStatusId.ToEntityReference(dfeta_teacherstatus.EntityLogicalName),
                dfeta_QTSDate = query.QtsDate
            }
        });

        return () => createResponse.GetResponse().id;
    }
}
