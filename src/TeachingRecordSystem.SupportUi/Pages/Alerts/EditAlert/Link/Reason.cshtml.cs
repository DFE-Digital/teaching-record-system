using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

[Journey(JourneyNames.EditAlertLink), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public const int MaxFileSizeMb = 50;
    public const int ChangeReasonDetailMaxLength = 4000;

    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.ChangeReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.HasAdditionalReasonDetail)
            .NotNull().WithMessage("Select yes if you want to add more information"),
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .MaximumLength(ChangeReasonDetailMaxLength)
                .WithMessage("Additional detail must be 4000 characters or less"),
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .NotEmpty().WithMessage("Enter additional detail")
            .When(m => m.HasAdditionalReasonDetail == true),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    public JourneyInstance<EditAlertLinkState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public AlertChangeLinkReasonOption? ChangeReason { get; set; }

    [BindProperty]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.AddLink is null)
        {
            context.Result = Redirect(linkGenerator.Alerts.EditAlert.Link.Index(AlertId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
    }

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance.State.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        Evidence = JourneyInstance.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await evidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);
        _validator.ValidateAndThrow(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail = HasAdditionalReasonDetail!.Value ? ChangeReasonDetail : null;
            state.Evidence = Evidence;
        });

        return Redirect(linkGenerator.Alerts.EditAlert.Link.CheckAnswers(AlertId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }
}
