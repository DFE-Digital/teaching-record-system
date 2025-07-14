using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class HoldsFromModel(IClock clock, TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public string? PersonName { get; set; }

    public Guid PersonId { get; set; }

    public RouteToProfessionalStatusType Route { get; set; } = null!;

    public RouteToProfessionalStatusStatus Status { get; set; }

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
            ModelState.AddModelError(nameof(HoldsFrom), "Professional Status Date must not be in the future");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var nextPage = JourneyInstance!.State.IsCompletingRoute ?
            NextCompletingRoutePage :
            FromCheckAnswers ?
                linkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
                linkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId);

        if (JourneyInstance!.State.IsCompletingRoute) // if user has set the status to 'holds' from another status
        {
            if (IsLastCompletingRoutePage()) // if this is the last page of the data collection for the status
            {
                await JourneyInstance!.UpdateStateAsync(s => // update the main journey state with the data
                {
                    s.Status = s.EditStatusState!.Status;
                    s.HoldsFrom = HoldsFrom;
                    s.IsExemptFromInduction = s.EditStatusState.InductionExemption;
                    s.EditStatusState = null;
                });
            }
            else // there are more pages to come - store the data in the temporary journey state
            {
                await JourneyInstance!.UpdateStateAsync(s => s.EditStatusState!.HoldsFrom = HoldsFrom);
            }
        }
        else // user is editing the Professional status 'hold' date on an already-completed route
        {
            await JourneyInstance!.UpdateStateAsync(s => s.HoldsFrom = HoldsFrom);
        }

        return Redirect(nextPage);
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        var routeFeature = context.HttpContext.GetCurrentProfessionalStatusFeature();
        Route = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);
        Status = JourneyInstance!.State.Status;
        var inductionexemptionReason = Route!.InductionExemptionReason;

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public string BackLink => FromCheckAnswers ?
            linkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
            JourneyInstance!.State.IsCompletingRoute ?
                linkGenerator.RouteEditStatus(QualificationId, JourneyInstance!.InstanceId) :
                linkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId);


    private bool IsLastCompletingRoutePage()
    {
        if (JourneyInstance!.State.EditStatusState != null)
        {
            if (QuestionDriverHelper.FieldRequired(Route!.InductionExemptionRequired, JourneyInstance!.State.EditStatusState.Status.GetInductionExemptionRequirement())
                == FieldRequirement.NotApplicable)
            {
                return true;
            }
            else
            {
                return Route.InductionExemptionReason is not null &&
                    Route.InductionExemptionReason.RouteImplicitExemption;
            }
        }
        else
        {
            return false;
        }
    }

    private string NextCompletingRoutePage =>
        IsLastCompletingRoutePage() ?
            linkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteEditInductionExemption(QualificationId, JourneyInstance!.InstanceId);
}
