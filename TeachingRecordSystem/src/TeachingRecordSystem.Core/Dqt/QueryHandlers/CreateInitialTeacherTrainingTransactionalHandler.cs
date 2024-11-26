using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateInitialTeacherTrainingTransactionalHandler : ICrmTransactionalQueryHandler<CreateInitialTeacherTrainingTransactionalQuery, Guid>
{
    public Func<Guid> AppendQuery(CreateInitialTeacherTrainingTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = query.Id,
                dfeta_PersonId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_CountryId = query.CountryId?.ToEntityReference(dfeta_country.EntityLogicalName),
                dfeta_ITTQualificationId = query.ITTQualificationId?.ToEntityReference(dfeta_qualification.EntityLogicalName),
                dfeta_Result = query.Result
            }
        });
        return () => createResponse.GetResponse().id;
    }
}
