using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class CrmTestData
{
    public Task<CreateNameChangeIncidentResult> CreateNameChangeIncident(Action<CreateNameChangeIncidentBuilder>? configure = null)
    {
        var builder = new CreateNameChangeIncidentBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class CreateNameChangeIncidentBuilder
    {
        private string _evidenceFileName = "evidence.txt";
        private MemoryStream _evidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test file"));
        private string _evidenceFileMimeType = "text/plain";
        private IncidentStatusType _incidentStatusType = IncidentStatusType.Active;

        public CreateNameChangeIncidentBuilder WithCanceledStatus()
        {
            _incidentStatusType = IncidentStatusType.Canceled;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithRejectedStatus()
        {
            _incidentStatusType = IncidentStatusType.Rejected;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithApprovedStatus()
        {
            _incidentStatusType = IncidentStatusType.Approved;
            return this;
        }

        public async Task<CreateNameChangeIncidentResult> Execute(CrmTestData testData)
        {
            var person = await testData.CreatePerson();

            var firstName = testData.GenerateFirstName();
            var middleName = testData.GenerateMiddleName();
            var lastName = testData.GenerateChangedLastName(person.LastName);

            var incidentId = Guid.NewGuid();
            var title = "Request to change name";
            var subjectTitle = "Change of Name";
            var nameChangeSubject = await testData.ReferenceDataCache.GetSubjectByTitle(subjectTitle);

            var incident = new Incident()
            {
                Id = incidentId,
                Title = title,
                SubjectId = nameChangeSubject!.Id.ToEntityReference(Subject.EntityLogicalName),
                CustomerId = person.ContactId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_NewFirstName = firstName,
                dfeta_NewMiddleName = middleName,
                dfeta_NewLastName = lastName,
                dfeta_StatedFirstName = firstName,
                dfeta_StatedMiddleName = middleName,
                dfeta_StatedLastName = lastName
            };

            var document = new dfeta_document()
            {
                Id = Guid.NewGuid(),
                dfeta_name = _evidenceFileName,
                dfeta_Type = dfeta_DocumentType.ChangeofNameDOBEvidence,
                dfeta_PersonId = person.ContactId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_CaseId = incidentId.ToEntityReference(Incident.EntityLogicalName),
                StatusCode = dfeta_document_StatusCode.Active
            };

            var annotationBody = await GetBase64EncodedFileContent(_evidenceFileContent);

            var annotation = new Annotation()
            {
                ObjectId = document.Id.ToEntityReference(dfeta_document.EntityLogicalName),
                ObjectTypeCode = dfeta_document.EntityLogicalName,
                Subject = _evidenceFileName,
                DocumentBody = annotationBody,
                MimeType = _evidenceFileMimeType,
                FileName = _evidenceFileName,
                NoteText = string.Empty
            };

            var txnRequestBuilder = RequestBuilder.CreateTransaction(testData.OrganizationService);
            txnRequestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = incident });
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = document });
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = annotation });
            switch (_incidentStatusType)
            {
                case IncidentStatusType.Canceled:
                    txnRequestBuilder.AddRequest(
                        new UpdateRequest()
                        {
                            Target = new Incident()
                            {
                                Id = incidentId,
                                StateCode = IncidentState.Canceled,
                                StatusCode = Incident_StatusCode.Canceled
                            }
                        });
                    break;
                case IncidentStatusType.Approved:
                    txnRequestBuilder.AddRequest(
                        new CloseIncidentRequest()
                        {
                            IncidentResolution = new IncidentResolution()
                            {
                                IncidentId = new EntityReference(Incident.EntityLogicalName, incidentId),
                                Subject = "Approved",
                            },
                            Status = new OptionSetValue((int)Incident_StatusCode.Approved),
                        });
                    break;
                case IncidentStatusType.Rejected:
                    txnRequestBuilder.AddRequest(
                        new CloseIncidentRequest()
                        {
                            IncidentResolution = new IncidentResolution()
                            {
                                IncidentId = new EntityReference(Incident.EntityLogicalName, incidentId),
                                Subject = "Rejected",
                            },
                            Status = new OptionSetValue((int)Incident_StatusCode.Rejected),
                        });
                    break;
                default:
                    break;
            }

            await txnRequestBuilder.Execute();

            return new CreateNameChangeIncidentResult()
            {
                IncidentId = incidentId,
                CustomerId = person.ContactId,
                Title = title,
                SubjectId = nameChangeSubject.Id,
                SubjectTitle = subjectTitle,
                CurrentFirstName = person.FirstName,
                CurrentMiddleName = person.MiddleName,
                CurrentLastName = person.LastName,
                NewFirstName = firstName,
                NewMiddleName = middleName,
                NewLastName = lastName,
                StatedFirstName = firstName,
                StatedMiddleName = middleName,
                StatedLastName = lastName,
                EvidenceFileName = _evidenceFileName,
                EvidenceBase64EncodedFileContent = annotationBody,
                EvidenceFileMimeType = _evidenceFileMimeType
            };
        }

        private enum IncidentStatusType
        {
            Active,
            Canceled,
            Approved,
            Rejected
        }
    }

    public record CreateNameChangeIncidentResult
    {
        public required Guid IncidentId { get; init; }
        public required Guid CustomerId { get; init; }
        public required string Title { get; init; }
        public required Guid SubjectId { get; init; }
        public required string SubjectTitle { get; init; }
        public required string CurrentFirstName { get; init; }
        public required string? CurrentMiddleName { get; init; }
        public required string CurrentLastName { get; init; }
        public required string NewFirstName { get; init; }
        public required string? NewMiddleName { get; init; }
        public required string NewLastName { get; init; }
        public required string StatedFirstName { get; init; }
        public required string? StatedMiddleName { get; init; }
        public required string StatedLastName { get; init; }
        public required string EvidenceFileName { get; init; }
        public required string EvidenceBase64EncodedFileContent { get; init; }
        public required string EvidenceFileMimeType { get; init; }
    }
}
