using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class InductionExemptionModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }
    public RouteToProfessionalStatusType Route { get; set; } = null!;

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

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

        if (JourneyInstance!.State.IsCompletingRoute) // if user has set the status to awarded or approved from another status
        {
            // this is definitely the final page of the data collection for an awarded or approved status
            await JourneyInstance!.UpdateStateAsync(s =>
            {
                s.Status = s.EditStatusState!.Status;
                s.TrainingEndDate = s.EditStatusState.TrainingEndDate.HasValue ? s.EditStatusState.TrainingEndDate.Value : s.TrainingEndDate;
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
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            linkGenerator.RouteEditDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;
        var professionalStatusFeature = context.HttpContext.GetCurrentProfessionalStatusFeature();
        Route = professionalStatusFeature!.RouteToProfessionalStatus.RouteToProfessionalStatusType!;

        if (Route.InductionExemptionRequired == FieldRequirement.NotApplicable
        || Route.InductionExemptionReason is not null && Route.InductionExemptionReason.RouteImplicitExemption)
        {
            context.Result = new BadRequestResult();
        }
        return base.OnPageHandlerExecutionAsync(context, next);
    }

    public string BackLink => FromCheckAnswers ?
        linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
        JourneyInstance!.State.IsCompletingRoute ?
            linkGenerator.RouteEditHoldsFrom(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId);
}
