using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ChangeRequests.EditChangeRequest;

[Authorize(Policy = AuthorizationPolicies.ChangeRequestManagement)]
public partial class IndexModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public IndexModel(ICrmQueryDispatcher crmQueryDispatcher)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    public string? ChangeType { get; set; }

    public string? PersonName { get; set; }

    public NameChangeRequestInfo? NameChangeRequest { get; set; }

    public DateOfBirthChangeRequestInfo? DateOfBirthChangeRequest { get; set; }

    public EvidenceInfo[]? Evidence { get; set; }

    [FromRoute]
    public string TicketNumber { get; set; } = null!;

    public async Task<IActionResult> OnGet()
    {
        var incidentDetail = await _crmQueryDispatcher.ExecuteQuery(new GetIncidentByTicketNumberQuery(TicketNumber));
        if (incidentDetail is null)
        {
            return NotFound();
        }

        if (incidentDetail.Incident.StateCode != IncidentState.Active)
        {
            return BadRequest();
        }

        SetModelFromIncidentDetail(incidentDetail);

        return Page();
    }
    public async Task<IActionResult> OnGetDocuments(Guid id)
    {
        var document = await _crmQueryDispatcher.ExecuteQuery(new GetDocumentByIdQuery(id));
        var annotation = document?.Extract<Annotation>("annotation", Annotation.PrimaryIdAttribute);

        if (document is null || annotation is null)
        {
            return NotFound();
        }

        if (document.StateCode != dfeta_documentState.Active)
        {
            return BadRequest();
        }

        var bytes = Convert.FromBase64String(annotation.DocumentBody);
        return File(bytes, annotation.MimeType);
    }

    private void SetModelFromIncidentDetail(IncidentDetail incidentDetail)
    {
        var incident = incidentDetail.Incident;
        var customer = incidentDetail.Contact;
        var subject = incidentDetail.Subject;
        var incidentDocuments = incidentDetail.IncidentDocuments;

        ChangeType = subject.Title;
        PersonName = customer.ResolveFullName(includeMiddleName: false);

        if (subject.Title == DqtConstants.NameChangeSubjectTitle)
        {
            NameChangeRequest = new NameChangeRequestInfo()
            {
                CurrentFirstName = customer.FirstName,
                CurrentMiddleName = customer.MiddleName,
                CurrentLastName = customer.LastName,
                NewFirstName = incident.dfeta_NewFirstName,
                NewMiddleName = incident.dfeta_NewMiddleName,
                NewLastName = incident.dfeta_NewLastName
            };
        }

        if (subject.Title == DqtConstants.DateOfBirthChangeSubjectTitle)
        {
            DateOfBirthChangeRequest = new DateOfBirthChangeRequestInfo()
            {
                CurrentDateOfBirth = customer.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false)!.Value,
                NewDateOfBirth = incident.dfeta_NewDateofBirth.ToDateOnlyWithDqtBstFix(isLocalTime: true)!.Value
            };
        }

        if (incidentDocuments.Length > 0)
        {
            var evidence = new List<EvidenceInfo>();
            foreach (var incidentDocument in incidentDocuments)
            {
                evidence.Add(new EvidenceInfo()
                {
                    DocumentId = incidentDocument.Document.dfeta_documentId!.Value,
                    FileName = incidentDocument.Annotation.FileName,
                    MimeType = incidentDocument.Annotation.MimeType
                });
            }

            Evidence = evidence.ToArray();
        }
    }

    public record EvidenceInfo
    {
        public required Guid DocumentId { get; init; }
        public required string FileName { get; init; }
        public required string MimeType { get; init; }
    }
}
