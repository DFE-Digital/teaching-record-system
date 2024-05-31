using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateDateOfBirthChangeIncidentHandler : ICrmQueryHandler<CreateDateOfBirthChangeIncidentQuery, (Guid IncidentId, string TicketNumber)>
{
    private readonly ReferenceDataCache _referenceDataCache;

    public CreateDateOfBirthChangeIncidentHandler(ReferenceDataCache referenceDataCache)
    {
        _referenceDataCache = referenceDataCache;
    }

    public async Task<(Guid IncidentId, string TicketNumber)> Execute(CreateDateOfBirthChangeIncidentQuery query, IOrganizationServiceAsync organizationService)
    {
        var subject = await _referenceDataCache.GetSubjectByTitle("Change of Date of Birth");

        var incident = new Incident()
        {
            Id = Guid.NewGuid(),
            Title = "Request to change date of birth",
            SubjectId = subject.Id.ToEntityReference(Subject.EntityLogicalName),
            CustomerId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_NewDateofBirth = query.DateOfBirth.ToDateTime(),
            dfeta_FromIdentity = query.FromIdentity
        };

        var document = new dfeta_document()
        {
            Id = Guid.NewGuid(),
            dfeta_name = query.EvidenceFileName,
            dfeta_Type = dfeta_DocumentType.ChangeofNameDOBEvidence,
            dfeta_PersonId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_CaseId = incident.Id.ToEntityReference(Incident.EntityLogicalName),
            StatusCode = dfeta_document_StatusCode.Active
        };

        var annotationBody = await StreamHelper.GetBase64EncodedFileContent(query.EvidenceFileContent);

        var annotation = new Annotation()
        {
            ObjectId = document.Id.ToEntityReference(dfeta_document.EntityLogicalName),
            ObjectTypeCode = dfeta_document.EntityLogicalName,
            Subject = query.EvidenceFileName,
            DocumentBody = annotationBody,
            MimeType = query.EvidenceFileMimeType,
            FileName = query.EvidenceFileName,
            NoteText = string.Empty
        };

        var requestBuilder = RequestBuilder.CreateTransaction(organizationService);
        requestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = incident });
        requestBuilder.AddRequest(new CreateRequest() { Target = document });
        requestBuilder.AddRequest(new CreateRequest() { Target = annotation });
        var getIncidentResponse = requestBuilder.AddRequest<RetrieveResponse>(
            new RetrieveRequest()
            {
                Target = incident.Id.ToEntityReference(Incident.EntityLogicalName),
                ColumnSet = new(Incident.Fields.TicketNumber)
            });
        requestBuilder.AddRequest(new UpdateRequest() { Target = new Contact() { Id = query.ContactId, dfeta_AllowPiiUpdatesFromRegister = false } });
        await requestBuilder.Execute();

        var ticketNumber = getIncidentResponse.GetResponse().Entity.ToEntity<Incident>().TicketNumber;
        return (incident.Id, ticketNumber);
    }
}
