using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class InductionExemptionModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string BackLink => FromCheckAnswers ?
        LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        LinkGenerator.RouteAddPage(PreviousPage(AddRoutePage.InductionExemption) ?? AddRoutePage.Status, PersonId, JourneyInstance!.InstanceId);

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

        await JourneyInstance!.UpdateStateAsync(s => s.IsExemptFromInduction = IsExemptFromInduction);

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteAddPage(NextPage(AddRoutePage.InductionExemption) ?? AddRoutePage.CheckYourAnswers, PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance!.State.RouteToProfessionalStatusId is null)
        {
            context.Result = BadRequest();
            return;
        }

        Route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId.Value);
        if (Route.InductionExemptionRequired == FieldRequirement.NotApplicable
            || (Route.InductionExemptionReason is not null && Route.InductionExemptionReason.RouteImplicitExemption))
        {
            context.Result = BadRequest();
            return;
        }

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
