using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateHeQualificationTransactionalHandler : ICrmTransactionalQueryHandler<CreateHeQualificationTransactionalQuery, Guid>
{
    public Func<Guid> AppendQuery(CreateHeQualificationTransactionalQuery query, RequestBuilder requestBuilder)
    {
        var createResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest()
        {
            Target = new dfeta_qualification()
            {
                Id = query.Id,
                dfeta_HE_CountryId = query.HECountryId?.ToEntityReference(dfeta_country.EntityLogicalName),
                dfeta_HE_ClassDivision = query.HEClassDivision,
                dfeta_HE_HEQualificationId = query.HEQualificationId?.ToEntityReference(dfeta_hequalification.EntityLogicalName),
                dfeta_Type = query.Type,
                dfeta_PersonId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_HE_EstablishmentId = query.HEEstablishmentId?.ToEntityReference(Account.EntityLogicalName),
                dfeta_HE_HESubject1Id = query.HESubject1id?.ToEntityReference(dfeta_hesubject.EntityLogicalName),
                dfeta_HE_HESubject2Id = query.HESubject2id?.ToEntityReference(dfeta_hesubject.EntityLogicalName),
                dfeta_HE_HESubject3Id = query.HESubject3id?.ToEntityReference(dfeta_hesubject.EntityLogicalName),
            }
        });
        return () => createResponse.GetResponse().id;
    }
}
