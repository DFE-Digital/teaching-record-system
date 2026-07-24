using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.AddPerson;

[Journey(JourneyNames.AddPerson)]
public class ReasonModel(
    AddPersonJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.Reason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ReasonDetail)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Reason details {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.ReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.Reason == PersonCreateReason.AnotherReason),
        v => v.RuleFor(m => m.Evidence).Evidence(),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotNull().WithMessage("Enter details")
            .MaximumLength(4000).WithMessage("Additional detail must be 4000 characters or less")
            .When(x => x.ProvideAdditionalInformation == ProvideMoreInformationOption.Yes)
    };

    public string? BackLink { get; set; }

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public PersonCreateReason? Reason { get; set; }

    [BindProperty]
    public string? ReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    [BindProperty]
    public ProvideMoreInformationOption? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? AdditionalInformation { get; set; }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        BackLink = journey.GetBackLink();
    }

    public void OnGet()
    {
        Reason = journey.State.Reason;
        ReasonDetail = journey.State.ReasonDetail;
        Evidence = journey.State.Evidence;
        ProvideAdditionalInformation = journey.State.ProvideAdditionalInformation;
        AdditionalInformation = journey.State.AdditionalInformation;
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

        return journey.AdvanceTo(
            linkGenerator.Persons.AddPerson.CheckAnswers(journey.InstanceId),
            state =>
            {
                state.Reason = Reason;
                state.ReasonDetail = Reason is PersonCreateReason.AnotherReason ? ReasonDetail : null;
                state.Evidence = Evidence;
                state.ProvideAdditionalInformation = ProvideAdditionalInformation;
                state.AdditionalInformation = ProvideAdditionalInformation == ProvideMoreInformationOption.Yes ? AdditionalInformation : null;
            });
    }
}
