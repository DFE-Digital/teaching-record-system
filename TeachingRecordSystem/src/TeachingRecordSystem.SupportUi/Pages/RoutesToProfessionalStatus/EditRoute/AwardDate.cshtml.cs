using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class AwardDateModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
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

        var nextPage = CompletingRoute ?
            (await NextCompletingRoutePageAsync()) :
            FromCheckAnswers ?
                linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
                linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId);

        if (CompletingRoute)
        {
            if (await LastCompletingRoutePageAsync())
            {
                await JourneyInstance!.UpdateStateAsync(s =>
                {
                    s.Status = s.EditStatusState!.Status;
                    s.TrainingEndDate = s.EditStatusState?.TrainingEndDate;
                    s.AwardedDate = AwardedDate;
                    s.IsExemptFromInduction = s.EditStatusState?.InductionExemption;
                    s.EditStatusState = null;
                });
            }
            else
            {
                await JourneyInstance!.UpdateStateAsync(s => s.EditStatusState!.AwardedDate = AwardedDate);
            }
        }
        else
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
        base.OnPageHandlerExecuting(context);
    }

    // Detail, end-date, CYA 
    public string BackLink => FromCheckAnswers ?
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
            CompletingRoute ?
                PreviousCompletingRoutePage() :
                linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId);


    private bool CompletingRoute => JourneyInstance!.State.EditStatusState != null;

    private async Task<bool> LastCompletingRoutePageAsync()
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
                    (await referenceDataCache.GetInductionExemptionReasonByIdAsync(RouteToProfessionalStatus.InductionExemptionReasonId!.Value)).RouteImplicitExemption;
            }
        }
        else
        {
            return false;
        }
    }

    private async Task<string> NextCompletingRoutePageAsync()
    {
        return !(await LastCompletingRoutePageAsync()) ?
            linkGenerator.RouteEditInductionExemption(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId);
    }

    private string PreviousCompletingRoutePage()
    {
        return QuestionDriverHelper.FieldRequired(RouteToProfessionalStatus!.TrainingEndDateRequired, JourneyInstance!.State.EditStatusState!.Status.GetEndDateRequirement()) != FieldRequirement.NotApplicable ?
            linkGenerator.RouteEditEndDate(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteEditStatus(QualificationId, JourneyInstance!.InstanceId);
    }
}
