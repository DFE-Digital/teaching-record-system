using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.DeleteAlert;

[Journey(JourneyNames.DeleteAlert), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<IndexModel> _validator = new()
    {
        v => v.RuleFor(m => m.DeleteReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.HasAdditionalReasonDetail)
            .NotNull().WithMessage("Select yes if you want to add more information"),
        v => v.RuleFor(m => m.DeleteReasonDetail)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.DeleteReasonDetail)
            .NotEmpty().WithMessage("Enter additional detail")
            .When(m => m.HasAdditionalReasonDetail == true),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    public JourneyInstance<DeleteAlertState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? AlertTypeName { get; set; }

    public DateOnly? EndDate { get; set; }

    [BindProperty]
    public DeleteAlertReasonOption? DeleteReason { get; set; }

    [BindProperty]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    public string? DeleteReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public void OnGet()
    {
        DeleteReason = JourneyInstance!.State.DeleteReason;
        HasAdditionalReasonDetail = JourneyInstance!.State.HasAdditionalReasonDetail;
        DeleteReasonDetail = JourneyInstance!.State.DeleteReasonDetail;
        Evidence = JourneyInstance!.State.Evidence;
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
            state.DeleteReason = DeleteReason;
            state.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.DeleteReasonDetail = DeleteReasonDetail;
            state.Evidence = Evidence;
        });

        return Redirect(linkGenerator.Alerts.DeleteAlert.CheckAnswers(AlertId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(EndDate is null ? linkGenerator.Persons.PersonDetail.Alerts(PersonId) : linkGenerator.Alerts.AlertDetail(AlertId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        AlertTypeName = alertInfo.Alert.AlertType!.Name;
        EndDate = alertInfo.Alert.EndDate;
    }
}
