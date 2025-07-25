using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class InductionExemptionModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : EditRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string PageTitle = "Edit induction exemption";

    [BindProperty]
    [Display(Name = "Does this route provide them with an induction exemption?")]
    [Required(ErrorMessage = "Select yes if this route provides an induction exemption")]
    public bool? IsExemptFromInduction { get; set; }

    public void OnGet()
    {
        IsExemptFromInduction = JourneyInstance!.State.IsExemptFromInduction;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        if (JourneyInstance!.State.IsCompletingRoute) // if user has set the status to 'holds' from another status
        {
            // this is definitely the final page of the data collection for a 'holds' status
            await JourneyInstance!.UpdateStateAsync(s =>
            {
                s.Status = s.EditStatusState!.Status;
                s.HoldsFrom = s.EditStatusState.HoldsFrom;
                s.IsExemptFromInduction = IsExemptFromInduction;
                s.EditStatusState = null;
            });
        }
        else // user is simply editing the induction exemption question
        {
            await JourneyInstance!.UpdateStateAsync(s => s.IsExemptFromInduction = IsExemptFromInduction);
        }

        return Redirect(FromCheckAnswers ?
            LinkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            LinkGenerator.RouteEditDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (RouteType!.InductionExemptionRequired == FieldRequirement.NotApplicable ||
            RouteType.InductionExemptionReason is not null && RouteType.InductionExemptionReason.RouteImplicitExemption)
        {
            context.Result = new BadRequestResult();
        }
    }

    public string BackLink => FromCheckAnswers ?
        LinkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
        JourneyInstance!.State.IsCompletingRoute ?
            LinkGenerator.RouteEditHoldsFrom(QualificationId, JourneyInstance!.InstanceId) :
            LinkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId);
}
