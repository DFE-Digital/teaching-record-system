using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckProfessionalStatusExistsFilterFactory()]
public class InductionExemptionModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }
    public RouteToProfessionalStatus Route { get; set; } = null!;

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
                s.AwardedDate = s.EditStatusState.AwardedDate;
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
            linkGenerator.RouteDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public async override Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        Route = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);
        if (Route.InductionExemptionReasonId is not null)
        {
            var exemptionReason = await referenceDataCache.GetInductionExemptionReasonByIdAsync(Route.InductionExemptionReasonId!.Value);
            if (exemptionReason.RouteImplicitExemption)
            {
                context.Result = new BadRequestResult();
            }
        }
        else
        {
            context.Result = new BadRequestResult();
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        await base.OnPageHandlerExecutionAsync(context, next);
    }

    public string BackLink => FromCheckAnswers ?
        linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
        JourneyInstance!.State.IsCompletingRoute ?
            linkGenerator.RouteEditAwardDate(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId);
}
