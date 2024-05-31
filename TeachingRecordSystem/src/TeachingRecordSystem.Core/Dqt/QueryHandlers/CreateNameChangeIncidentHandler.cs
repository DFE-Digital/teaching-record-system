using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Core.Dqt.QueryHandlers;

public class CreateNameChangeIncidentHandler : ICrmQueryHandler<CreateNameChangeIncidentQuery, (Guid IncidentId, string TicketNumber)>
{
    private readonly ReferenceDataCache _referenceDataCache;

    public CreateNameChangeIncidentHandler(ReferenceDataCache referenceDataCache)
    {
        _referenceDataCache = referenceDataCache;
    }

    public async Task<(Guid IncidentId, string TicketNumber)> Execute(CreateNameChangeIncidentQuery query, IOrganizationServiceAsync organizationService)
    {
        var subject = await _referenceDataCache.GetSubjectByTitle("Change of Name");

        var incident = new Incident()
        {
            Id = Guid.NewGuid(),
            Title = "Request to change name",
            SubjectId = subject.Id.ToEntityReference(Subject.EntityLogicalName),
            CustomerId = query.ContactId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_NewFirstName = query.FirstName,
            dfeta_NewMiddleName = query.MiddleName,
            dfeta_NewLastName = query.LastName,
            dfeta_StatedFirstName = query.StatedFirstName,
            dfeta_StatedMiddleName = query.StatedMiddleName,
            dfeta_StatedLastName = query.StatedLastName,
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
