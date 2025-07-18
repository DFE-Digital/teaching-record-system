using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class InductionExemptionModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRoutePostStatusPageModel(AddRoutePage.InductionExemption, linkGenerator, referenceDataCache)
{
    public string PageTitle = "Add induction exemption";

    [BindProperty]
    [Display(Name = "Does this route provide them with an induction exemption?")]
    [Required(ErrorMessage = "Select yes if this route provides an induction exemption")]
    public bool? IsExemptFromInduction { get; set; }

    public IActionResult OnGet()
    {
        IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.IsExemptFromInduction = IsExemptFromInduction;
        });

        return await ContinueAsync();
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (RouteType.InductionExemptionRequired == FieldRequirement.NotApplicable
            || (RouteType.InductionExemptionReason is not null && RouteType.InductionExemptionReason.RouteImplicitExemption))
        {
            context.Result = BadRequest();
            return;
        }
    }
}
