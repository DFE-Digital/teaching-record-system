using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt.Tests;

public partial class TestDataHelper
{
    public async Task<CreateNameChangeIncidentResult> CreateNameChangeIncident(Guid customerId)
    {
        var lookupRequestBuilder = _dataverseAdapter.CreateMultipleRequestBuilder();

        var nameChangeSubjectTitle = "Change of Name";
        var getNameChangeSubjectTask = _globalCache.GetOrCreateAsync(
            CacheKeys.GetSubjectTitleKey(nameChangeSubjectTitle),
            _ => _dataverseAdapter.GetSubjectByTitle(nameChangeSubjectTitle, lookupRequestBuilder));

        await lookupRequestBuilder.Execute();
        var nameChangeSubject = getNameChangeSubjectTask.Result;

        var title = "Request to change name";
        var newFirstName = Faker.Name.First();        
        var newMiddleName = Faker.Name.Middle();
        var newLastName = Faker.Name.Last();
        var evidenceFileName = "evidence.txt";
        var evidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test file"));
        var evidenceFileMimeType = "text/plain";

        var incident = new Incident()
        {
            Id = Guid.NewGuid(),
            Title = title,
            SubjectId = nameChangeSubject!.Id.ToEntityReference(Subject.EntityLogicalName),
            CustomerId = customerId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_NewFirstName = newFirstName,
            dfeta_NewMiddleName = newMiddleName,
            dfeta_NewLastName = newLastName,
            dfeta_StatedFirstName = newFirstName,
            dfeta_StatedMiddleName = newMiddleName,
            dfeta_StatedLastName = newLastName
        };

        var document = new dfeta_document()
        {
            Id = Guid.NewGuid(),
            dfeta_name = evidenceFileName,
            dfeta_Type = dfeta_DocumentType.ChangeofNameDOBEvidence,
            dfeta_PersonId = customerId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_CaseId = incident.Id.ToEntityReference(Incident.EntityLogicalName),
            StatusCode = dfeta_document_StatusCode.Active
        };

        var annotationBody = await GetBase64EncodedFileContent(evidenceFileContent);

        var annotation = new Annotation()
        {
            ObjectId = document.Id.ToEntityReference(dfeta_document.EntityLogicalName),
            ObjectTypeCode = dfeta_document.EntityLogicalName,
            Subject = evidenceFileName,
            DocumentBody = annotationBody,
            MimeType = evidenceFileMimeType,
            FileName = evidenceFileName,
            NoteText = string.Empty
        };

        var txnRequestBuilder = _dataverseAdapter.CreateTransactionRequestBuilder();
        var createIncidentResponse = txnRequestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = incident });
        txnRequestBuilder.AddRequest(new CreateRequest() { Target = document });
        txnRequestBuilder.AddRequest(new CreateRequest() { Target = annotation });
        await txnRequestBuilder.Execute();
        var incidentId = createIncidentResponse.GetResponse().id;

        return new CreateNameChangeIncidentResult(
            incidentId,
            customerId,
            title,
            nameChangeSubject.Id,
            newFirstName,
            newMiddleName,
            newLastName,
            newFirstName,
            newMiddleName,
            newLastName,
            evidenceFileName,
            annotationBody,
            evidenceFileMimeType);
    }

    private static async Task<string> GetBase64EncodedFileContent(Stream file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var buffer = ms.ToArray();
        return Convert.ToBase64String(buffer);
    }
}

public record CreateNameChangeIncidentResult(
    Guid IncidentId,
    Guid CustomerId,
    string Title,
    Guid SubjectId,
    string NewFirstName,
    string? NewMiddleName,
    string NewLastName,
    string StatedFirstName,
    string? StatedMiddleName,
    string StatedLastName,
    string EvidenceFileName,
    string EvidenceBase64EncodedFileContent,
    string EvidenceFileMimeType);
