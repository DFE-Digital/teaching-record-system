﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;
using QualifiedTeachersApi.DataStore.Crm.Models;

namespace QualifiedTeachersApi.DataStore.Crm;

public partial class DataverseAdapter
{
    public async Task<Guid> CreateNameChangeIncident(CreateNameChangeIncidentCommand command)
    {
        var subject = await GetSubjectByTitle("Change of Name", columnNames: Array.Empty<string>());

        var incident = new Incident()
        {
            Id = Guid.NewGuid(),
            Title = "Request to change name",
            SubjectId = subject.Id.ToEntityReference(Subject.EntityLogicalName),
            CustomerId = command.ContactId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_NewFirstName = command.FirstName,
            dfeta_NewMiddleName = command.MiddleName,
            dfeta_NewLastName = command.LastName,
        };

        var document = new dfeta_document()
        {
            Id = Guid.NewGuid(),
            dfeta_name = command.EvidenceFileName,
            dfeta_Type = dfeta_DocumentType.ChangeofNameDOBEvidence,
            dfeta_PersonId = command.ContactId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_CaseId = incident.Id.ToEntityReference(Incident.EntityLogicalName),
            StatusCode = dfeta_document_StatusCode.Active
        };

        var annotationBody = await GetBase64EncodedFileContent(command.EvidenceFileContent);

        var annotation = new Annotation()
        {
            ObjectId = document.Id.ToEntityReference(dfeta_document.EntityLogicalName),
            ObjectTypeCode = dfeta_document.EntityLogicalName,
            Subject = command.EvidenceFileName,
            DocumentBody = annotationBody,
            MimeType = command.EvidenceFileMimeType,
            FileName = command.EvidenceFileName,
            NoteText = string.Empty
        };

        var requestBuilder = CreateTransactionRequestBuilder();
        var createIncidentResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = incident });
        requestBuilder.AddRequest(new CreateRequest() { Target = document });
        requestBuilder.AddRequest(new CreateRequest() { Target = annotation });
        await requestBuilder.Execute();

        return createIncidentResponse.GetResponse().id;
    }

    public async Task<Guid> CreateDateOfBirthChangeIncident(CreateDateOfBirthChangeIncidentCommand command)
    {
        var subject = await GetSubjectByTitle("Change of Date of Birth", columnNames: Array.Empty<string>());

        var incident = new Incident()
        {
            Id = Guid.NewGuid(),
            Title = "Request to change date of birth",
            SubjectId = subject.Id.ToEntityReference(Subject.EntityLogicalName),
            CustomerId = command.ContactId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_NewDateofBirth = command.DateOfBirth.ToDateTime()
        };

        var document = new dfeta_document()
        {
            Id = Guid.NewGuid(),
            dfeta_name = command.EvidenceFileName,
            dfeta_Type = dfeta_DocumentType.ChangeofNameDOBEvidence,
            dfeta_PersonId = command.ContactId.ToEntityReference(Contact.EntityLogicalName),
            dfeta_CaseId = incident.Id.ToEntityReference(Incident.EntityLogicalName),
            StatusCode = dfeta_document_StatusCode.Active
        };

        var annotationBody = await GetBase64EncodedFileContent(command.EvidenceFileContent);

        var annotation = new Annotation()
        {
            ObjectId = document.Id.ToEntityReference(dfeta_document.EntityLogicalName),
            ObjectTypeCode = dfeta_document.EntityLogicalName,
            Subject = command.EvidenceFileName,
            DocumentBody = annotationBody,
            MimeType = command.EvidenceFileMimeType,
            FileName = command.EvidenceFileName,
            NoteText = string.Empty
        };

        var requestBuilder = CreateTransactionRequestBuilder();
        var createIncidentResponse = requestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = incident });
        requestBuilder.AddRequest(new CreateRequest() { Target = document });
        requestBuilder.AddRequest(new CreateRequest() { Target = annotation });
        await requestBuilder.Execute();

        return createIncidentResponse.GetResponse().id;
    }

    private static async Task<string> GetBase64EncodedFileContent(Stream file)
    {
        var sb = new StringBuilder();

        var buffer = new byte[64 * 1024];

        int read;
        while ((read = await file.ReadAsync(buffer)) != 0)
        {
            var encoded = Convert.ToBase64String(buffer, 0, read);
            sb.Append(encoded);
        }

        return sb.ToString();
    }
}
