using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class HoldsFromModel(IClock clock, TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : EditRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public string PageTitle = "Enter the professional status date";
    public string PageHeading => PageTitle;

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Professional status date")]
    public DateOnly? HoldsFrom { get; set; }

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
                LinkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
                LinkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId);

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

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        var inductionexemptionReason = Route!.InductionExemptionReason;
    }

    public string BackLink => FromCheckAnswers ?
            LinkGenerator.RouteEditCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
            JourneyInstance!.State.IsCompletingRoute ?
                LinkGenerator.RouteEditStatus(QualificationId, JourneyInstance!.InstanceId) :
                LinkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId);


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
            LinkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId) :
            LinkGenerator.RouteEditInductionExemption(QualificationId, JourneyInstance!.InstanceId);
}
