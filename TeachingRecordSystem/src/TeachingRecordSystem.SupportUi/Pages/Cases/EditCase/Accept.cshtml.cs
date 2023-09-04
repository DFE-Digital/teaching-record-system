using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Cases.EditCase;

[Authorize(Policy = AuthorizationPolicies.CaseManagement)]
public class AcceptModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public AcceptModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    [FromRoute]
    public string TicketNumber { get; set; } = null!;

    public string? CurrentValue { get; set; }

    public string? NewValue { get; set; }

    public async Task<IActionResult> OnGet()
    {
        (Incident Incident, dfeta_document[] Documents)? incidentAndDocuments = await GetIncidentAndDocuments();
        if (incidentAndDocuments is null)
        {
            return NotFound();
        }

        if (incidentAndDocuments.Value.Incident.StateCode != IncidentState.Active)
        {
            return BadRequest();
        }

        SetModelFromIncident(incidentAndDocuments.Value.Incident);

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        (Incident Incident, dfeta_document[] Documents)? incidentAndDocuments = await GetIncidentAndDocuments();

        _ = await _crmQueryDispatcher.ExecuteQuery(new ApproveIncidentQuery(incidentAndDocuments.Value.Incident.Id));

        TempData.SetFlashSuccess(
            $"The request has been accepted",
            "The user’s record has been changed and they have been notified.");

        return Redirect(_linkGenerator.Cases());
    }

    private void SetModelFromIncident(Incident incident)
    {
        var customer = incident.Extract<Contact>("contact", Contact.PrimaryIdAttribute);
        var subject = incident.Extract<Subject>("subject", Subject.PrimaryIdAttribute);

        if (subject.Title == DqtConstants.NameChangeSubjectTitle)
        {
            CurrentValue = string.IsNullOrEmpty(customer.MiddleName) ? $"{customer.FirstName} {customer.LastName}" : $"{customer.FirstName} {customer.MiddleName} {customer.LastName}";
            NewValue = string.IsNullOrEmpty(incident.dfeta_NewMiddleName) ? $"{incident.dfeta_NewFirstName} {incident.dfeta_NewLastName}" : $"{incident.dfeta_NewFirstName} {incident.dfeta_NewMiddleName} {incident.dfeta_NewLastName}";
        }

        if (subject.Title == DqtConstants.DateOfBirthChangeSubjectTitle)
        {
            CurrentValue = customer.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false)!.Value.ToString("dd/MM/yyyy");
            NewValue = incident.dfeta_NewDateofBirth.ToDateOnlyWithDqtBstFix(isLocalTime: true)!.Value.ToString("dd/MM/yyyy");
        }
    }

    private Task<(Incident Incident, dfeta_document[] Documents)?> GetIncidentAndDocuments() =>
        _crmQueryDispatcher.ExecuteQuery(new GetIncidentByTicketNumberQuery(TicketNumber));
}
