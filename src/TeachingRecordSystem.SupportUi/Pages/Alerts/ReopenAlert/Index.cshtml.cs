using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.ReopenAlert;

[Journey(JourneyNames.ReopenAlert), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.ChangeReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.HasAdditionalReasonDetail)
            .NotNull().WithMessage("Select yes if you want to add more information about why you’re removing the end date"),
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .NotEmpty().WithMessage("Enter additional detail")
            .When(m => m.HasAdditionalReasonDetail == true),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    public JourneyInstance<ReopenAlertState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public ReopenAlertReasonOption? ChangeReason { get; set; }

    [BindProperty]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance.State.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await evidenceUploadManager.ValidateAndUploadAsync<IndexModel>(m => m.Evidence, ViewData);
        _validator.ValidateAndThrow(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail = ChangeReasonDetail;
            state.Evidence = Evidence;
        });

        return Redirect(linkGenerator.Alerts.ReopenAlert.CheckAnswers(AlertId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Alerts.AlertDetail(AlertId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
    }
}
