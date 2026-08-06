using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction)]
public class ReasonModel(
    EditInductionJourneyCoordinator journey,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.ChangeReason == PersonInductionChangeReason.AnotherReason),
        v => v.RuleFor(m => m.ChangeReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information about why you’re changing the induction details"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotEmpty().WithMessage("Enter details")
            .When(m => m.ProvideAdditionalInformation == true),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public PersonInductionChangeReason? ChangeReason { get; set; }

    [BindProperty]
    public bool? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public string? AdditionalInformation { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public void OnGet()
    {
        ChangeReason = journey.State.ChangeReason;
        ProvideAdditionalInformation = journey.State.ProvideAdditionalInformation;
        ChangeReasonDetail = journey.State.ChangeReason == PersonInductionChangeReason.AnotherReason ? journey.State.ChangeReasonDetail : null;
        AdditionalInformation = journey.State.ProvideAdditionalInformation == true ? journey.State.AdditionalInformation : null;
        Evidence = journey.State.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        // Upload the evidence file before validating so that it's retained if the form is re-rendered
        // with errors.
        await evidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);

        await this.ThrowIfInvalidAsync(_validator);

        journey.UpdateState(
            state =>
            {
                state.ChangeReason = ChangeReason;
                state.ProvideAdditionalInformation = ProvideAdditionalInformation;
                state.AdditionalInformation = ProvideAdditionalInformation is true ? AdditionalInformation : null;
                state.ChangeReasonDetail = ChangeReason == PersonInductionChangeReason.AnotherReason ? ChangeReasonDetail : null;
                state.Evidence = Evidence;
            });

        return Redirect(journey.ContinueTo(journey.CheckAnswersUrl()));
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        BackLink = journey.GetBackLink() ?? journey.InductionUrl;

        return base.OnPageHandlerExecutionAsync(context, next);
    }
}
