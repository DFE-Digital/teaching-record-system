using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckProfessionalStatusExistsFilterFactory()]
public class StatusModel(
    TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }
    public RouteToProfessionalStatus Route { get; set; } = null!;

    public ProfessionalStatusStatusInfo[] Statuses { get; set; } = [];

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    [Display(Name = "Select the route status")]
    [Required(ErrorMessage = "Select a route status")]
    public ProfessionalStatusStatus Status { get; set; }

    public void OnGet()
    {
        Status = JourneyInstance!.State.Status;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        if (CompletingRoute)
        {
            var hasImplicitExemption =
                Route.InductionExemptionReasonId.HasValue ?
                    (await referenceDataCache.GetInductionExemptionReasonByIdAsync(Route.InductionExemptionReasonId.Value)).RouteImplicitExemption
                    : false;
            await JourneyInstance!.UpdateStateAsync(
                s =>
                {
                    s.EditStatusState = new EditRouteStatusState
                    {
                        Status = Status,
                        RouteImplicitExemption = hasImplicitExemption,
                        InductionExemption = hasImplicitExemption ? true : null
                    };
                });
        }
        else if (NotCompletedRoute)
        {
            await JourneyInstance!.UpdateStateAsync(s =>
            {
                s.AwardedDate = null;
                s.IsExemptFromInduction = null;
                s.Status = Status;
            });
        }
        else // going from Approved to Awarded or vice versa
        {
            await JourneyInstance!.UpdateStateAsync(s => s.Status = Status);
        }

        return Redirect(CompletingRoute ?
            NextCompletingRoutePage(Status) :
            FromCheckAnswers ?
                linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
                linkGenerator.RouteDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        Statuses = ProfessionalStatusStatusRegistry.All.ToArray();
        Route = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        await next();
    }

    public string BackLink =>
         FromCheckAnswers ?
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId);

    private string NextCompletingRoutePage(ProfessionalStatusStatus status)
    {
        return (QuestionDriverHelper.FieldRequired(Route.TrainingEndDateRequired, status.GetEndDateRequirement()) != FieldRequirement.NotApplicable) ?
            linkGenerator.RouteEditEndDate(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RouteEditAwardDate(QualificationId, JourneyInstance!.InstanceId);
    }

    private bool CompletingRoute => Status == ProfessionalStatusStatus.Awarded && (JourneyInstance!.State.CurrentStatus != ProfessionalStatusStatus.Awarded && JourneyInstance!.State.CurrentStatus != ProfessionalStatusStatus.Approved)
        || Status == ProfessionalStatusStatus.Approved && (JourneyInstance!.State.CurrentStatus != ProfessionalStatusStatus.Approved && JourneyInstance!.State.CurrentStatus != ProfessionalStatusStatus.Awarded);

    public bool NotCompletedRoute => Status != ProfessionalStatusStatus.Approved && Status != ProfessionalStatusStatus.Awarded;

}
