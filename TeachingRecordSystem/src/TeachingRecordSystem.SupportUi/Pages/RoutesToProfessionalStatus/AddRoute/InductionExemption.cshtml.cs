using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class InductionExemptionModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    protected override RoutePage CurrentPage => RoutePage.InductionExemption;

    public string BackLink => PreviousPageUrl;

    [BindProperty]
    [Display(Name = "Does this route provide them with an induction exemption?")]
    [Required(ErrorMessage = "Select yes if this route provides an induction exemption")]
    public bool? IsExemptFromInduction { get; set; }

    public IActionResult OnGet()
    {
        IsExemptFromInduction = JourneyInstance!.State.NewRouteToProfessionalStatusId != null ? JourneyInstance!.State.NewIsExemptFromInduction : JourneyInstance!.State.IsExemptFromInduction;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(s =>
        {
            if (JourneyInstance!.State.NewRouteToProfessionalStatusId == null)
            {
                s.Begin();
            }

            s.NewIsExemptFromInduction = IsExemptFromInduction;
        });

        return await ContinueAsync();
    }

    protected override Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        if (Route.InductionExemptionRequired == FieldRequirement.NotApplicable
            || (Route.InductionExemptionReason is not null && Route.InductionExemptionReason.RouteImplicitExemption))
        {
            context.Result = BadRequest();
        }

        return Task.CompletedTask;
    }
}
