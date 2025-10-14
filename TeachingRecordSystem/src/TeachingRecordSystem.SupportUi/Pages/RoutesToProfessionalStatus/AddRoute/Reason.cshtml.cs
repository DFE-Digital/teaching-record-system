using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class ReasonModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache, EvidenceUploadManager evidenceUploadManager)
    : AddRoutePostStatusPageModel(AddRoutePage.ChangeReason, linkGenerator, referenceDataCache, evidenceUploadManager)
{
    [BindProperty]
    [Required(ErrorMessage = "Select a reason")]
    [Display(Name = "Why are you adding this route?")]
    public ChangeReasonOption? ChangeReason { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to provide more information?")]
    [Required(ErrorMessage = "Select yes if you want to add more information about why you\u2019re adding this route")]
    public bool? HasAdditionalReasonDetail { get; set; }

    [BindProperty]
    [Display(Name = "Enter details")]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Additional detail {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? ChangeReasonDetail { get; set; }

    [BindProperty]
    public EvidenceUploadModel Evidence { get; set; } = new();

    public void OnGet()
    {
        ChangeReason = JourneyInstance!.State.ChangeReason;
        HasAdditionalReasonDetail = JourneyInstance.State.ChangeReasonDetail.HasAdditionalReasonDetail;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail.ChangeReasonDetail;
        Evidence = JourneyInstance.State.ChangeReasonDetail.Evidence;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HasAdditionalReasonDetail == true && ChangeReasonDetail is null)
        {
            ModelState.AddModelError(nameof(ChangeReasonDetail), "Enter additional detail");
        }

        await EvidenceController.ValidateAndUploadAsync(Evidence, ModelState);

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.ChangeReason = ChangeReason;
            state.ChangeReasonDetail.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail.ChangeReasonDetail = ChangeReasonDetail;
            state.ChangeReasonDetail.HasAdditionalReasonDetail = HasAdditionalReasonDetail;
            state.ChangeReasonDetail.Evidence = Evidence;
        });

        return await ContinueAsync();
    }
}
