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

        if (CompletingRoute) // if user has set the status to awarded or approved from another status
        {
            // if the route has an implicit exemption it needs to be set now
            var hasImplicitExemption = Route.InductionExemptionReason?.RouteImplicitExemption ?? false;

            // initialise a temporary part of journey state for the data collection for a completing route
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
        else if (NotCompletedRoute) // the status has been changed to something other than awarded or approved
        {
            await JourneyInstance!.UpdateStateAsync(s =>
            {
                s.AwardedDate = null; // clear any previous awarded date and exemption
                s.IsExemptFromInduction = null;
                s.Status = Status;
            });
        }
        else // going from approved to awarded or vice versa
        {
            await JourneyInstance!.UpdateStateAsync(s => s.Status = Status);
        }

        return Redirect(CompletingRoute ?
            NextCompletingRoutePage(Status) :
            FromCheckAnswers ?
                linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
                linkGenerator.RouteEditDetail(QualificationId, JourneyInstance.InstanceId));
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
            linkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId);

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
