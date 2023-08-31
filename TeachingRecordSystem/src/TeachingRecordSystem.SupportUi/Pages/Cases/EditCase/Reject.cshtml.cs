using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Cases.EditCase;

[Authorize(Policy = AuthorizationPolicies.CaseManagement)]
public class RejectModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public RejectModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    [FromRoute]
    public string TicketNumber { get; set; } = null!;

    [BindProperty]
    [Display(Name = " ")]
    [Required(ErrorMessage = "Select the reason for rejecting this change")]
    public CaseRejectionReasonOption? RejectionReasonChoice { get; set; }

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

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        (Incident Incident, dfeta_document[] Documents)? incidentAndDocuments = await GetIncidentAndDocuments();

        var requestStatus = "rejected";
        var flashMessage = "The user's record has not been changed and they have been notified.";
        if (RejectionReasonChoice!.Value == CaseRejectionReasonOption.ChangeNoLongerRequired)
        {
            requestStatus = "cancelled";
            flashMessage = "The user's record has not been changed and they have not been notified.";

            _ = await _crmQueryDispatcher.ExecuteQuery(new CancelIncidentQuery(incidentAndDocuments.Value.Incident.Id));
        }
        else
        {
            _ = await _crmQueryDispatcher.ExecuteQuery(new RejectIncidentQuery(incidentAndDocuments.Value.Incident.Id, RejectionReasonChoice.Value.GetDisplayName()!));
        }

        TempData.SetFlashSuccess(
            $"The request has been {requestStatus}",
            flashMessage);

        return Redirect(_linkGenerator.Cases());
    }

    private Task<(Incident Incident, dfeta_document[] Documents)?> GetIncidentAndDocuments() =>
        _crmQueryDispatcher.ExecuteQuery(new GetIncidentByTicketNumberQuery(TicketNumber));

    public enum CaseRejectionReasonOption
    {
        [Display(Name = "Request and proof don't match")]
        RequestAndProofDontMatch,
        [Display(Name = "Wrong type of document")]
        WrongTypeOfDocument,
        [Display(Name = "Image quality")]
        ImageQuality,
        [Display(Name = "Change no longer required")]
        ChangeNoLongerRequired
    }
}
