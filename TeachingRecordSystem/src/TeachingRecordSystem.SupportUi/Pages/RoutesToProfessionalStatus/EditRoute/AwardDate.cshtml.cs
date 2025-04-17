using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class AwardDateModel(
    TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public string? PersonName { get; set; }

    public Guid PersonId { get; set; }

    public RouteToProfessionalStatus? RouteToProfessionalStatus { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Award date")]
    [Required(ErrorMessage = "Enter an award date")]
    [Display(Name = "Enter the professional status award date")]
    public DateOnly? AwardedDate { get; set; }

    public void OnGet()
    {
        AwardedDate = JourneyInstance!.State.AwardedDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        var nextPage = JourneyInstance!.State.IsCompletingRoute ?
            NextCompletingRoutePage :
            FromCheckAnswers ?
                linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
                linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId);

        if (JourneyInstance!.State.IsCompletingRoute) // if user has set the status to awarded or approved from another status
        {
            if (IsLastCompletingRoutePage()) // if this is the last page of the data collection for the status
            {
                await JourneyInstance!.UpdateStateAsync(s => // update the main journey state with the data
                {
                    s.Status = s.EditStatusState!.Status;
                    s.TrainingEndDate = s.EditStatusState.TrainingEndDate.HasValue ? s.EditStatusState.TrainingEndDate.Value : s.TrainingEndDate;
                    s.AwardedDate = AwardedDate;
                    s.IsExemptFromInduction = s.EditStatusState.InductionExemption;
                    s.EditStatusState = null;
                });
            }
            else // there are more pages to come - store the data in the temporary journey state
            {
                await JourneyInstance!.UpdateStateAsync(s => s.EditStatusState!.AwardedDate = AwardedDate);
            }
        }
        else // user is editing the awarded date on an already-completed route
        {
            await JourneyInstance!.UpdateStateAsync(s => s.AwardedDate = AwardedDate);
        }

        return Redirect(nextPage);
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        var routeFeature = context.HttpContext.GetCurrentProfessionalStatusFeature();
        RouteToProfessionalStatus = routeFeature.ProfessionalStatus.Route;
        var inductionexemptionReason = RouteToProfessionalStatus.InductionExemptionReason;
        base.OnPageHandlerExecuting(context);
    }

    public string BackLink => FromCheckAnswers ?
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
            JourneyInstance!.State.IsCompletingRoute ?
                PreviousCompletingRoutePage :
                linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId);


    private bool IsLastCompletingRoutePage()
    {
        if (JourneyInstance!.State.EditStatusState != null)
        {
            if (QuestionDriverHelper.FieldRequired(RouteToProfessionalStatus!.InductionExemptionRequired, JourneyInstance!.State.EditStatusState.Status.GetInductionExemptionRequirement())
                == FieldRequirement.NotApplicable)
            {
                return true;
            }
            else
            {
                return RouteToProfessionalStatus.InductionExemptionReasonId.HasValue &&
                    RouteToProfessionalStatus.InductionExemptionReason.RouteImplicitExemption;
            }
        }
        else
        {
            return false;
        }
    }

    private string NextCompletingRoutePage =>
        IsLastCompletingRoutePage() ?
            linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteEditInductionExemption(QualificationId, JourneyInstance!.InstanceId);

    private string PreviousCompletingRoutePage =>
        QuestionDriverHelper.FieldRequired(RouteToProfessionalStatus!.TrainingEndDateRequired, JourneyInstance!.State.EditStatusState!.Status.GetEndDateRequirement()) != FieldRequirement.NotApplicable ?
            linkGenerator.RouteEditEndDate(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteEditStatus(QualificationId, JourneyInstance!.InstanceId);
}
