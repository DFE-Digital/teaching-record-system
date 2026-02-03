using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Reject;

[Journey(JourneyNames.RejectNpqTrnRequest), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.RejectionReason)
            .NotNull().WithMessage("Select a reason for rejecting this request")
    };

    public string PersonName => ' '.JoinNonEmpty(RequestData!.Name);

    public TrnRequestMetadata? RequestData { get; set; }

    public JourneyInstance<RejectNpqTrnRequestState>? JourneyInstance { get; set; }

    [FromRoute]
    public string SupportTaskReference { get; init; } = null!;

    [FromQuery]
    public bool FromCheckAnswers { get; init; }

    [BindProperty]
    public RejectionReasonOption? RejectionReason { get; set; }

    public void OnGet()
    {
        RejectionReason = JourneyInstance!.State.RejectionReason;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _validator.ValidateAndThrow(this);

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.RejectionReason = RejectionReason;
        });

        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Reject.CheckAnswers(SupportTaskReference, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.SupportTasks.NpqTrnRequests.Index());
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var supportTask = HttpContext.GetCurrentSupportTaskFeature().SupportTask;
        RequestData = supportTask.TrnRequestMetadata!;

        return base.OnPageHandlerExecutionAsync(context, next);
    }
}
