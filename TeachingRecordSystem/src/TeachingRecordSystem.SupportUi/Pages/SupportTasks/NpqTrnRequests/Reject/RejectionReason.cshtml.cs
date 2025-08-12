using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;

[Journey(JourneyNames.RejectNpqTrnRequest), RequireJourneyInstance, ActivatesJourney]
public class RejectionReasonModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public string PersonName => StringHelper.JoinNonEmpty(' ', RequestData!.Name);

    public TrnRequestMetadata? RequestData { get; set; } // CML TODO - needed? or just name needed?

    public JourneyInstance<RejectNpqTrnRequestState>? JourneyInstance { get; set; }

    [FromRoute]
    public string SupportTaskReference { get; set; } = null!;

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Why are you rejecting this request?")]
    [Required(ErrorMessage = "Select a reason for rejecting this request")]
    public RejectionReasonOption? RejectionReason { get; set; }

    public void OnGet()
    {
        RejectionReason = JourneyInstance!.State.RejectionReason;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.RejectionReason = RejectionReason;
        });

        return Redirect(linkGenerator.NpqTrnRequestRejectionCheckAnswers(SupportTaskReference, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.NpqTrnRequests());
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        RequestData = supportTask.TrnRequestMetadata!;

        return base.OnPageHandlerExecutionAsync(context, next);
    }
}
