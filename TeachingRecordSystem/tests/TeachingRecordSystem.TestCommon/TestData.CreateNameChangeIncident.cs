using System.Diagnostics;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<CreateNameChangeIncidentResult> CreateNameChangeIncident(Action<CreateNameChangeIncidentBuilder> configure)
    {
        var builder = new CreateNameChangeIncidentBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class CreateNameChangeIncidentBuilder
    {
        private const IncidentStatusType DefaultIncidentStatus = IncidentStatusType.Active;

        private static readonly string _defaultEvidenceFileName = "evidence1.jpeg";
        private static readonly MemoryStream _defaultEvidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test image"));
        private static readonly string _defaultEvidenceFileMimeType = "image/jpeg";

        private static readonly string _additionalEvidenceFileName = "evidence2.pdf";
        private static readonly MemoryStream _additionalEvidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test PDF"));
        private static readonly string _additionalEvidenceFileMimeType = "application/pdf";

        private Guid? _customerId;
        private string? _newFirstName;
        private string? _newMiddleName;
        private string? _newLastName;
        private IncidentStatusType? _incidentStatusType;
        private bool _hasMultipleEvidenceFiles = false;

        public CreateNameChangeIncidentBuilder WithCustomerId(Guid customerId)
        {
            if (_customerId is not null && _customerId != customerId)
            {
                throw new InvalidOperationException("Customer ID cannot be changed after it's set.");
            }

            _customerId = customerId;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithNewFirstName(string firstName)
        {
            if (_newFirstName is not null && _newFirstName != firstName)
            {
                throw new InvalidOperationException("New first name cannot be changed after it's set.");
            }

            _newFirstName = firstName;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithNewMiddleName(string middleName)
        {
            if (_newMiddleName is not null && _newMiddleName != middleName)
            {
                throw new InvalidOperationException("New middle name cannot be changed after it's set.");
            }

            _newMiddleName = middleName;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithNewLastName(string lastName)
        {
            if (_newLastName is not null && _newLastName != lastName)
            {
                throw new InvalidOperationException("New last name cannot be changed after it's set.");
            }

            _newLastName = lastName;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithMultipleEvidenceFiles()
        {
            _hasMultipleEvidenceFiles = true;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithStatus(IncidentStatusType status)
        {
            if (_incidentStatusType is not null && _incidentStatusType != status)
            {
                throw new InvalidOperationException("Incident status cannot be changed after it's set.");
            }

            _incidentStatusType = status;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithCanceledStatus() => WithStatus(IncidentStatusType.Canceled);

        public CreateNameChangeIncidentBuilder WithRejectedStatus() => WithStatus(IncidentStatusType.Rejected);

        public CreateNameChangeIncidentBuilder WithApprovedStatus() => WithStatus(IncidentStatusType.Approved);

        public async Task<CreateNameChangeIncidentResult> Execute(TestData testData)
        {
            if (_customerId is null)
            {
                throw new InvalidOperationException("Customer ID must be specified.");
            }

            var firstName = _newFirstName ?? testData.GenerateFirstName();
            var middleName = _newMiddleName ?? testData.GenerateMiddleName();
            var lastName = _newLastName ?? testData.GenerateLastName();

            var incidentId = Guid.NewGuid();
            var title = "Request to change name";
            var subjectTitle = "Change of Name";
            var nameChangeSubject = await testData.ReferenceDataCache.GetSubjectByTitle(subjectTitle);

            var incident = new Incident()
            {
                Id = incidentId,
                Title = title,
                SubjectId = nameChangeSubject!.Id.ToEntityReference(Subject.EntityLogicalName),
                CustomerId = _customerId.Value.ToEntityReference(Contact.EntityLogicalName),
                dfeta_NewFirstName = firstName,
                dfeta_NewMiddleName = middleName,
                dfeta_NewLastName = lastName,
                dfeta_StatedFirstName = firstName,
                dfeta_StatedMiddleName = middleName,
                dfeta_StatedLastName = lastName
            };

            var evidences = new List<CreateNameChangeIncidentEvidence>();

            var txnRequestBuilder = RequestBuilder.CreateTransaction(testData.OrganizationService);
            txnRequestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = incident });
            var (document, annotation, evidence) = await CreateDocument(_customerId.Value, _defaultEvidenceFileName, _defaultEvidenceFileContent, _defaultEvidenceFileMimeType);
            evidences.Add(evidence);
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = document });
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = annotation });
            if (_hasMultipleEvidenceFiles)
            {
                (document, annotation, evidence) = await CreateDocument(_customerId.Value, _additionalEvidenceFileName, _additionalEvidenceFileContent, _additionalEvidenceFileMimeType);
                evidences.Add(evidence);
                txnRequestBuilder.AddRequest(new CreateRequest() { Target = document });
                txnRequestBuilder.AddRequest(new CreateRequest() { Target = annotation });
            }

            _incidentStatusType ??= DefaultIncidentStatus;

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
                    Debug.Assert(_incidentStatusType == IncidentStatusType.Active);
                    break;
            }

            var retrieveIncidentResponse = txnRequestBuilder.AddRequest<RetrieveResponse>(
                new RetrieveRequest()
                {
                    Target = incidentId.ToEntityReference(Incident.EntityLogicalName),
                    ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(Incident.Fields.TicketNumber, Incident.Fields.CreatedOn)
                });

            await txnRequestBuilder.Execute();

            var createdIncident = retrieveIncidentResponse.GetResponse().Entity.ToEntity<Incident>();
            var ticketNumber = createdIncident.TicketNumber;
            var createdOn = createdIncident.CreatedOn!.Value;

            return new CreateNameChangeIncidentResult()
            {
                IncidentId = incidentId,
                TicketNumber = ticketNumber,
                CreatedOn = createdOn,
                CustomerId = _customerId.Value,
                Title = title,
                SubjectId = nameChangeSubject.Id,
                SubjectTitle = subjectTitle,
                NewFirstName = firstName,
                NewMiddleName = middleName,
                NewLastName = lastName,
                StatedFirstName = firstName,
                StatedMiddleName = middleName,
                StatedLastName = lastName,
                Evidence = evidences.ToArray()
            };

            async Task<(dfeta_document, Annotation, CreateNameChangeIncidentEvidence)> CreateDocument(Guid customerId, string filename, Stream content, string mimeType)
            {
                var document = new dfeta_document()
                {
                    Id = Guid.NewGuid(),
                    dfeta_name = filename,
                    dfeta_Type = dfeta_DocumentType.ChangeofNameDOBEvidence,
                    dfeta_PersonId = customerId.ToEntityReference(Contact.EntityLogicalName),
                    dfeta_CaseId = incidentId.ToEntityReference(Incident.EntityLogicalName),
                    StatusCode = dfeta_document_StatusCode.Active
                };

                var annotationBody = await GetBase64EncodedFileContent(content);

                var annotation = new Annotation()
                {
                    ObjectId = document.Id.ToEntityReference(dfeta_document.EntityLogicalName),
                    ObjectTypeCode = dfeta_document.EntityLogicalName,
                    Subject = filename,
                    DocumentBody = annotationBody,
                    MimeType = mimeType,
                    FileName = filename,
                    IsDocument = true,
                    NoteText = string.Empty
                };

                var evidence = new CreateNameChangeIncidentEvidence()
                {
                    DocumentId = document.Id,
                    FileName = filename,
                    Base64EncodedFileContent = annotationBody,
                    MimeType = mimeType
                };

                return (document, annotation, evidence);
            }
        }

        public enum IncidentStatusType
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
        public required string TicketNumber { get; init; }
        public required DateTime CreatedOn { get; init; }
        public required Guid CustomerId { get; init; }
        public required string Title { get; init; }
        public required Guid SubjectId { get; init; }
        public required string SubjectTitle { get; init; }
        public required string NewFirstName { get; init; }
        public required string? NewMiddleName { get; init; }
        public required string NewLastName { get; init; }
        public required string StatedFirstName { get; init; }
        public required string? StatedMiddleName { get; init; }
        public required string StatedLastName { get; init; }
        public required CreateNameChangeIncidentEvidence[] Evidence { get; init; }
    }

    public record CreateNameChangeIncidentEvidence
    {
        public required Guid DocumentId { get; init; }
        public required string FileName { get; init; }
        public required string Base64EncodedFileContent { get; init; }
        public required string MimeType { get; init; }
    }
}
