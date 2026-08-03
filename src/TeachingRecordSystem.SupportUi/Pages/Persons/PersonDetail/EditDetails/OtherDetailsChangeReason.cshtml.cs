using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditDetails;

[Journey(JourneyNames.EditDetails)]
public class OtherDetailsChangeReasonModel(
    EditDetailsJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<OtherDetailsChangeReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.Reason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ReasonDetail)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Reason details {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.ReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.Reason == PersonDetailsChangeReason.AnotherReason),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    public string? BackLink { get; set; }

    [FromRoute]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public PersonDetailsChangeReason? Reason { get; set; }

    [BindProperty]
    public string? ReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public bool IsAlsoChangingName => journey.State.NameChangeReason is not null;

    public void OnGet()
    {
        Reason = journey.State.OtherDetailsChangeReason;
        ReasonDetail = journey.State.OtherDetailsChangeReasonDetail;
        Evidence = journey.State.OtherDetailsChangeEvidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        // Upload the evidence file before validating so that it's retained if the form is re-rendered
        // with errors.
        await evidenceUploadManager.UploadAsync(Evidence);

        await _validator.ValidateAndThrowAsync(this);

        return journey.AdvanceToNextQuestion(
            linkGenerator.Persons.PersonDetail.EditDetails.CheckAnswers(journey.InstanceId),
            state =>
            {
                state.OtherDetailsChangeReason = Reason;
                state.OtherDetailsChangeReasonDetail = Reason is PersonDetailsChangeReason.AnotherReason ? ReasonDetail : null;
                state.OtherDetailsChangeEvidence = Evidence;
            });
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;

        BackLink = journey.GetBackLink();
    }
}
