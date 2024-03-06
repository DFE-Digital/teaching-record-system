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
    public Task<CreateDateOfBirthChangeIncidentResult> CreateDateOfBirthChangeIncident(Action<CreateDateOfBirthChangeIncidentBuilder>? configure = null)
    {
        var builder = new CreateDateOfBirthChangeIncidentBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class CreateDateOfBirthChangeIncidentBuilder
    {
        private const IncidentStatusType DefaultIncidentStatus = IncidentStatusType.Active;

        private static readonly string _defaultEvidenceFileName = "evidence.jpeg";
        private static readonly MemoryStream _defaultEvidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test file"));
        private static readonly string _defaultEvidenceFileMimeType = "image/jpeg";

        private Guid? _customerId;
        private IncidentStatusType? _incidentStatusType;

        public CreateDateOfBirthChangeIncidentBuilder WithCustomerId(Guid customerId)
        {
            if (_customerId is not null && _customerId != customerId)
            {
                throw new InvalidOperationException("Customer ID cannot be changed after it's set.");
            }

            _customerId = customerId;
            return this;
        }

        public CreateDateOfBirthChangeIncidentBuilder WithStatus(IncidentStatusType status)
        {
            if (_incidentStatusType is not null && _incidentStatusType != status)
            {
                throw new InvalidOperationException("Incident status cannot be changed after it's set.");
            }

            _incidentStatusType = status;
            return this;
        }

        public CreateDateOfBirthChangeIncidentBuilder WithCanceledStatus() => WithStatus(IncidentStatusType.Canceled);

        public CreateDateOfBirthChangeIncidentBuilder WithRejectedStatus() => WithStatus(IncidentStatusType.Rejected);

        public CreateDateOfBirthChangeIncidentBuilder WithApprovedStatus() => WithStatus(IncidentStatusType.Approved);

        public async Task<CreateDateOfBirthChangeIncidentResult> Execute(TestData testData)
        {
            if (_customerId is null)
            {
                throw new InvalidOperationException("Customer ID must be specified.");
            }

            var dateOfBirth = testData.GenerateDateOfBirth();

            var incidentId = Guid.NewGuid();
            var title = "Request to change date of birth";
            var subjectTitle = "Change of Date of Birth";
            var dateOfBirthChangeSubject = await testData.ReferenceDataCache.GetSubjectByTitle(subjectTitle);

            var incident = new Incident()
            {
                Id = incidentId,
                Title = title,
                SubjectId = dateOfBirthChangeSubject!.Id.ToEntityReference(Subject.EntityLogicalName),
                CustomerId = _customerId.Value.ToEntityReference(Contact.EntityLogicalName),
                dfeta_NewDateofBirth = dateOfBirth.ToDateTime()
            };

            var document = new dfeta_document()
            {
                Id = Guid.NewGuid(),
                dfeta_name = _defaultEvidenceFileName,
                dfeta_Type = dfeta_DocumentType.ChangeofNameDOBEvidence,
                dfeta_PersonId = _customerId.Value.ToEntityReference(Contact.EntityLogicalName),
                dfeta_CaseId = incidentId.ToEntityReference(Incident.EntityLogicalName),
                StatusCode = dfeta_document_StatusCode.Active
            };

            var annotationBody = await GetBase64EncodedFileContent(_defaultEvidenceFileContent);

            var annotation = new Annotation()
            {
                ObjectId = document.Id.ToEntityReference(dfeta_document.EntityLogicalName),
                ObjectTypeCode = dfeta_document.EntityLogicalName,
                Subject = _defaultEvidenceFileName,
                DocumentBody = annotationBody,
                MimeType = _defaultEvidenceFileMimeType,
                FileName = _defaultEvidenceFileName,
                IsDocument = true,
                NoteText = string.Empty
            };

            var txnRequestBuilder = RequestBuilder.CreateTransaction(testData.OrganizationService);
            txnRequestBuilder.AddRequest<CreateResponse>(new CreateRequest() { Target = incident });
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = document });
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = annotation });

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

            return new CreateDateOfBirthChangeIncidentResult()
            {
                IncidentId = incidentId,
                TicketNumber = ticketNumber,
                CreatedOn = createdOn,
                CustomerId = _customerId.Value,
                Title = title,
                SubjectId = dateOfBirthChangeSubject.Id,
                SubjectTitle = subjectTitle,
                NewDateOfBirth = dateOfBirth,
                Evidence = new CreateDateOfBirthChangeIncidentEvidence()
                {
                    DocumentId = document.Id,
                    FileName = _defaultEvidenceFileName,
                    Base64EncodedFileContent = annotationBody,
                    MimeType = _defaultEvidenceFileMimeType
                }
            };
        }

        public enum IncidentStatusType
        {
            Active,
            Canceled,
            Approved,
            Rejected
        }
    }

    public record CreateDateOfBirthChangeIncidentResult
    {
        public required Guid IncidentId { get; init; }
        public required string TicketNumber { get; init; }
        public required DateTime CreatedOn { get; init; }
        public required Guid CustomerId { get; init; }
        public required string Title { get; init; }
        public required Guid SubjectId { get; init; }
        public required string SubjectTitle { get; init; }
        public required DateOnly NewDateOfBirth { get; init; }
        public required CreateDateOfBirthChangeIncidentEvidence Evidence { get; init; }
    }

    public record CreateDateOfBirthChangeIncidentEvidence
    {
        public required Guid DocumentId { get; init; }
        public required string FileName { get; init; }
        public required string Base64EncodedFileContent { get; init; }
        public required string MimeType { get; init; }
    }
}
