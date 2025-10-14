using Microsoft.AspNetCore.Mvc;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class HoldsFromModel(
    IClock clock,
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceUploadManager)
    : AddRoutePostStatusPageModel(AddRoutePage.HoldsFrom, linkGenerator, referenceDataCache, evidenceUploadManager)
{
    [BindProperty]
    [DateInput(ErrorMessagePrefix = "The date they first held this professional status")]
    public DateOnly? HoldsFrom { get; set; }

    public bool HoldsFromRequired => QuestionDriverHelper.FieldRequired(RouteType!.HoldsFromRequired, Status.GetHoldsFromDateRequirement())
        == FieldRequirement.Mandatory;

    public string PageHeading => "Enter the date they first held this professional status"
       + (HoldsFromRequired ? "" : " (optional)");

    public void OnGet()
    {
        HoldsFrom = JourneyInstance!.State.HoldsFrom;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HoldsFromRequired && HoldsFrom is null)
        {
            ModelState.AddModelError(nameof(HoldsFrom), "Enter the date they first held this professional status");
        }

        if (HoldsFrom > clock.Today)
        {
            ModelState.AddModelError(nameof(HoldsFrom), "The date they first held this professional status must not be in the future");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.HoldsFrom = HoldsFrom;
        });

        return await ContinueAsync();
    }
}
