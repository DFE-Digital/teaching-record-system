using System.Diagnostics;
using System.Text;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.TestCommon;

public partial class CrmTestData
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

        private static readonly string _defaultEvidenceFileName = "evidence.txt";
        private static readonly MemoryStream _defaultEvidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test file"));
        private static readonly string _defaultEvidenceFileMimeType = "text/plain";

        private Guid? _customerId;
        private IncidentStatusType? _incidentStatusType;

        public CreateNameChangeIncidentBuilder WithCustomerId(Guid customerId)
        {
            if (_customerId is not null && _customerId != customerId)
            {
                throw new InvalidOperationException("Customer ID cannot be changed after it's set.");
            }

            _customerId = customerId;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithCanceledStatus()
        {
            if (_incidentStatusType is not null && _incidentStatusType != IncidentStatusType.Canceled)
            {
                throw new InvalidOperationException("Incident status cannot be changed after it's set.");
            }

            _incidentStatusType = IncidentStatusType.Canceled;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithRejectedStatus()
        {
            if (_incidentStatusType is not null && _incidentStatusType != IncidentStatusType.Rejected)
            {
                throw new InvalidOperationException("Incident status cannot be changed after it's set.");
            }

            _incidentStatusType = IncidentStatusType.Rejected;
            return this;
        }

        public CreateNameChangeIncidentBuilder WithApprovedStatus()
        {
            if (_incidentStatusType is not null && _incidentStatusType != IncidentStatusType.Approved)
            {
                throw new InvalidOperationException("Incident status cannot be changed after it's set.");
            }

            _incidentStatusType = IncidentStatusType.Approved;
            return this;
        }

        public async Task<CreateNameChangeIncidentResult> Execute(CrmTestData testData)
        {
            if (_customerId is null)
            {
                throw new InvalidOperationException("Customer ID must be specified.");
            }

            var firstName = testData.GenerateFirstName();
            var middleName = testData.GenerateMiddleName();
            var lastName = testData.GenerateLastName();

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
                EvidenceFileName = _defaultEvidenceFileName,
                EvidenceBase64EncodedFileContent = annotationBody,
                EvidenceFileMimeType = _defaultEvidenceFileMimeType
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
        public required string EvidenceFileName { get; init; }
        public required string EvidenceBase64EncodedFileContent { get; init; }
        public required string EvidenceFileMimeType { get; init; }
    }
}
