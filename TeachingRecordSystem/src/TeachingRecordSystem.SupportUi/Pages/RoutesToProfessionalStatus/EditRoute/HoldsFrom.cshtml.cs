using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class HoldsFromModel(
    IClock clock,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceUploadManager)
    : EditRouteCommonPageModel(linkGenerator, referenceDataCache, evidenceUploadManager)
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

        var nextPage = JourneyInstance!.State.IsCompletingRoute ?
            NextCompletingRoutePage :
            FromCheckAnswers ?
                LinkGenerator.RoutesToProfessionalStatus.EditRoute.CheckAnswers(QualificationId, JourneyInstance!.InstanceId) :
                LinkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(QualificationId, JourneyInstance!.InstanceId);

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

        var inductionexemptionReason = RouteType!.InductionExemptionReason;
    }

    public string BackLink => FromCheckAnswers ?
            LinkGenerator.RoutesToProfessionalStatus.EditRoute.CheckAnswers(QualificationId, JourneyInstance!.InstanceId) :
            JourneyInstance!.State.IsCompletingRoute ?
                LinkGenerator.RoutesToProfessionalStatus.EditRoute.Status(QualificationId, JourneyInstance!.InstanceId) :
                LinkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(QualificationId, JourneyInstance!.InstanceId);


    private bool IsLastCompletingRoutePage()
    {
        if (JourneyInstance!.State.EditStatusState != null)
        {
            if (QuestionDriverHelper.FieldRequired(RouteType!.InductionExemptionRequired, JourneyInstance!.State.EditStatusState.Status.GetInductionExemptionRequirement())
                == FieldRequirement.NotApplicable)
            {
                return true;
            }
            else
            {
                return RouteType.InductionExemptionReason is not null &&
                    RouteType.InductionExemptionReason.RouteImplicitExemption;
            }
        }
        else
        {
            return false;
        }
    }

    private string NextCompletingRoutePage =>
        IsLastCompletingRoutePage() ?
            LinkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(QualificationId, JourneyInstance!.InstanceId) :
            LinkGenerator.RoutesToProfessionalStatus.EditRoute.InductionExemption(QualificationId, JourneyInstance!.InstanceId);
}
