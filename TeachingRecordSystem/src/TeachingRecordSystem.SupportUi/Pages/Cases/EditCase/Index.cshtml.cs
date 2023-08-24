using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Cases.EditCase;

[Authorize(Roles = $"{UserRoles.Helpdesk},{UserRoles.Administrator}")]
public class IndexModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public IndexModel(ICrmQueryDispatcher crmQueryDispatcher)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    public CaseInfo? CaseHeader { get; set; }

    public NameChangeRequestInfo? NameChangeRequest { get; set; }

    public DateOfBirthChangeRequestInfo? DateOfBirthChangeRequest { get; set; }

    public EvidenceInfo? Evidence { get; set; }

    [FromRoute]
    public string TicketNumber { get; set; } = null!;

    public async Task<IActionResult> OnGet()
    {
        var incident = await _crmQueryDispatcher.ExecuteQuery(new GetIncidentByTicketNumberQuery(TicketNumber));
        if (incident is null)
        {
            return NotFound();
        }

        if (incident.StateCode != IncidentState.Active)
        {
            return BadRequest();
        }

        SetModelFromIncident(incident);

        return Page();
    }

    private void SetModelFromIncident(Incident incident)
    {
        var customer = incident.Extract<Contact>("contact", Contact.PrimaryIdAttribute);
        var subject = incident.Extract<Subject>("subject", Subject.PrimaryIdAttribute);
        var document = incident.Extract<dfeta_document>("dfeta_document", dfeta_document.PrimaryIdAttribute);
        var annotation = document?.Extract<Annotation>("annotation", Annotation.PrimaryIdAttribute);

        CaseHeader = new CaseInfo()
        {
            CaseReference = incident.TicketNumber,
            Customer = customer.dfeta_StatedFirstName is not null ? $"{customer.dfeta_StatedFirstName} {customer.dfeta_StatedLastName}" : $"{customer.FirstName} {customer.LastName}",
            CaseType = subject.Title,
            CreatedOn = incident.CreatedOn!.Value
        };

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

        if (document is not null && annotation is not null)
        {
            Evidence = new EvidenceInfo()
            {
                DocumentId = document!.dfeta_documentId!.Value,
                DocumentName = annotation.FileName,
                MimeType = annotation.MimeType
            };
        }
    }

    public record CaseInfo
    {
        public required string CaseReference { get; init; }
        public required string Customer { get; init; }
        public required string CaseType { get; init; }
        public required DateTime CreatedOn { get; init; }
    }

    public record NameChangeRequestInfo
    {
        public required string CurrentFirstName { get; init; }
        public required string? CurrentMiddleName { get; init; }
        public required string CurrentLastName { get; init; }
        public required string NewFirstName { get; init; }
        public required string? NewMiddleName { get; init; }
        public required string NewLastName { get; init; }
    }

    public record DateOfBirthChangeRequestInfo
    {
        public required DateOnly CurrentDateOfBirth { get; init; }
        public required DateOnly NewDateOfBirth { get; init; }
    }

    public record EvidenceInfo
    {
        public required Guid DocumentId { get; init; }
        public required string DocumentName { get; init; }
        public required string MimeType { get; init; }
    }
}
