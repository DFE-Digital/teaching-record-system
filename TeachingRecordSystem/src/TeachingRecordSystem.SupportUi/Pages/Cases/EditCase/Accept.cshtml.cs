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
        var incidentDetail = await GetIncidentDetail();
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

    public async Task<IActionResult> OnPost()
    {
        var incidentDetail = await GetIncidentDetail();

        _ = await _crmQueryDispatcher.ExecuteQuery(new ApproveIncidentQuery(incidentDetail!.Incident.Id));

        TempData.SetFlashSuccess(
            $"The request has been accepted",
            "The user’s record has been changed and they have been notified.");

        return Redirect(_linkGenerator.Cases());
    }

    private void SetModelFromIncidentDetail(IncidentDetail incidentDetail)
    {
        var incident = incidentDetail.Incident;
        var customer = incidentDetail.Contact;
        var subject = incidentDetail.Subject;

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

    private Task<IncidentDetail?> GetIncidentDetail() =>
        _crmQueryDispatcher.ExecuteQuery(new GetIncidentByTicketNumberQuery(TicketNumber));
}
