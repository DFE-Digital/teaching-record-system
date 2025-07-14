using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class HoldsFromModel(IClock clock, TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRoutePostStatusPageModel(AddRoutePage.HoldsFrom, linkGenerator, referenceDataCache)
{
    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Professional status date")]
    [Display(Name = "Enter the professional status date")]
    public DateOnly? HoldsFrom { get; set; }

    public string PageHeading => "Enter the professional status date" + (!HoldsFromRequired ? " (optional)" : "");
    public bool HoldsFromRequired => QuestionDriverHelper.FieldRequired(Route!.HoldsFromRequired, Status.GetHoldsFromDateRequirement())
        == FieldRequirement.Mandatory;

    public void OnGet()
    {
        HoldsFrom = JourneyInstance!.State.HoldsFrom;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (HoldsFromRequired && HoldsFrom is null)
        {
            ModelState.AddModelError(nameof(HoldsFrom), "Enter a professional status date");
        }

        if (HoldsFrom > clock.Today)
        {
            ModelState.AddModelError(nameof(HoldsFrom), "Professional status date must not be in the future");
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
