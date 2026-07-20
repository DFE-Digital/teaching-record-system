using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[TeachingRecordSystem.WebCommon.FormFlow.Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class ReasonModel(SupportUiLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache, EvidenceUploadManager evidenceUploadManager)
    : AddRoutePostStatusPageModel(AddRoutePage.ChangeReason, linkGenerator, referenceDataCache, evidenceUploadManager)
{
    private readonly InlineValidator<ReasonModel> _validator = new()
    {
        v => v.RuleFor(m => m.ChangeReason)
            .NotNull().WithMessage("Select a reason"),
        v => v.RuleFor(m => m.ChangeReasonDetail)
            .NotEmpty().WithMessage("Enter a reason")
            .When(m => m.ChangeReason == ChangeReasonOption.AnotherReason),
        v => v.RuleFor(m => m.ProvideAdditionalInformation)
            .NotNull().WithMessage("Select yes if you want to add more information about why you\u2019re adding this route"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .MaximumLength(UiDefaults.ReasonDetailsMaxCharacterCount)
                .WithMessage($"Additional detail {UiDefaults.ReasonDetailsMaxCharacterCountErrorMessage}"),
        v => v.RuleFor(m => m.AdditionalInformation)
            .NotEmpty().WithMessage("Enter detail")
            .When(m => m.ProvideAdditionalInformation == ProvideMoreInformationOption.Yes),
        v => v.RuleFor(m => m.Evidence).Evidence()
    };

    [BindProperty]
    public ChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    public ProvideMoreInformationOption? ProvideAdditionalInformation { get; set; }

    [BindProperty]
    public string? AdditionalInformation { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    [BindProperty]
    public string? ChangeReasonDetail { get; set; }

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ProvideAdditionalInformation = JourneyInstance.State.ChangeReasonDetail.ProvideAdditionalInformation;
        AdditionalInformation = JourneyInstance.State.ChangeReasonDetail.AdditionalInformation;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail.ChangeReasonDetail;
        Evidence = JourneyInstance.State.ChangeReasonDetail.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await EvidenceUploadManager.ValidateAndUploadAsync<ReasonModel>(m => m.Evidence, ViewData);
        _validator.ValidateAndThrow(this);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail.ChangeReasonDetail = ChangeReasonDetail;
            state.ChangeReasonDetail.ProvideAdditionalInformation = ProvideAdditionalInformation;
            state.ChangeReasonDetail.AdditionalInformation = AdditionalInformation;
            state.ChangeReasonDetail.Evidence = Evidence;
        });

        return await ContinueAsync();
    }
}
