using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus)]
public class ReasonModel(
    EditRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    EvidenceUploadManager evidenceUploadManager) : PageModel
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.ChangeReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.ChangeReason == ChangeReasonOption.AnotherReason),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information about why you\u2019re editing this route"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotEmpty().WithMessage("Enter details")
            .When(m => m.ProvideAdditionalInformation == ProvideMoreInformationOption.Yes),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    public string PageCaption => journey.PageCaption;

    public string BackLink => journey.GetReturnUrlOrDefault(DetailUrl);

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public ChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    public ProvideMoreInformationOption? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public string? AdditionalInformation { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    private string DetailUrl => linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(journey.InstanceId);

    public void OnGet()
    {
        ChangeReason = journey.State.ChangeReason;
        ProvideAdditionalInformation = journey.State.ChangeReasonDetail.ProvideAdditionalInformation;
        ChangeReasonDetail = journey.State.ChangeReasonDetail.ChangeReasonDetail;
        AdditionalInformation = journey.State.ChangeReasonDetail.AdditionalInformation;
        Evidence = journey.State.ChangeReasonDetail.Evidence;
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

        journey.UpdateState(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail.ProvideAdditionalInformation = ProvideAdditionalInformation;
            state.ChangeReasonDetail.ChangeReasonDetail = ChangeReasonDetail;
            state.ChangeReasonDetail.AdditionalInformation = AdditionalInformation;
            state.ChangeReasonDetail.Evidence = Evidence;
        });

        return Redirect(linkGenerator.RoutesToProfessionalStatus.EditRoute.CheckAnswers(journey.InstanceId));
    }
}
