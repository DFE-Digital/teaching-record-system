using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.ChangeRequests.EditChangeRequest;

[Authorize(Policy = AuthorizationPolicies.ChangeRequestManagement)]
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

    public string? ChangeType { get; set; }

    public string? PersonName { get; set; }

    IncidentDetail? IncidentDetail { get; set; }

    [BindProperty]
    [Display(Name = " ")]
    [Required(ErrorMessage = "Select the reason for rejecting this change")]
    public CaseRejectionReasonOption? RejectionReasonChoice { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var requestStatus = "rejected";
        var flashMessage = "The user’s record has not been changed and they have been notified.";
        if (RejectionReasonChoice!.Value == CaseRejectionReasonOption.ChangeNoLongerRequired)
        {
            requestStatus = "cancelled";
            flashMessage = "The user’s record has not been changed and they have not been notified.";

            _ = await _crmQueryDispatcher.WithDqtUserImpersonation().ExecuteQuery(new CancelIncidentQuery(IncidentDetail!.Incident.Id));
        }
        else
        {
            _ = await _crmQueryDispatcher.WithDqtUserImpersonation().ExecuteQuery(new RejectIncidentQuery(IncidentDetail!.Incident.Id, RejectionReasonChoice.Value.GetDisplayName()!));
        }

        TempData.SetFlashSuccess(
            $"The request has been {requestStatus}",
            flashMessage);

        return Redirect(_linkGenerator.SupportTasks());
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        IncidentDetail = await _crmQueryDispatcher.WithDqtUserImpersonation().ExecuteQuery(new GetIncidentByTicketNumberQuery(TicketNumber));
        if (IncidentDetail is null)
        {
            context.Result = NotFound();
            return;
        }

        if (IncidentDetail.Incident.StateCode != IncidentState.Active)
        {
            context.Result = BadRequest();
            return;
        }

        ChangeType = IncidentDetail.Subject.Title;
        PersonName = IncidentDetail.Contact.ResolveFullName(includeMiddleName: false);

        await next();
    }

    public enum CaseRejectionReasonOption
    {
        [Display(Name = "Request and proof don’t match")]
        RequestAndProofDontMatch,
        [Display(Name = "Wrong type of document")]
        WrongTypeOfDocument,
        [Display(Name = "Image quality")]
        ImageQuality,
        [Display(Name = "Change no longer required")]
        ChangeNoLongerRequired
    }
}
