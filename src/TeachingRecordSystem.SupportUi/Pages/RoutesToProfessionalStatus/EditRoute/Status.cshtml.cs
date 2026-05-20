using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckRouteToProfessionalStatusExistsFilterFactory()]
public class StatusModel(
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    private readonly InlineValidator<StatusModel> _validator = new()
    {
        v => v.RuleFor(m => m.Status)
            .IsInEnum().WithMessage("Select a route status")
    };

    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    public RouteToProfessionalStatusType Route { get; set; } = null!;

    public ProfessionalStatusStatusInfo[] Statuses { get; set; } = [];

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    public RouteToProfessionalStatusStatus Status { get; set; }

    public void OnGet()
    {
        Status = JourneyInstance!.State.EditStatusState is null ? JourneyInstance!.State.Status : JourneyInstance!.State.EditStatusState.Status;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _validator.ValidateAndThrow(this);

        if (CompletingRoute) // if user has set the status to 'holds' from another status
        {
            // if the route has an implicit exemption it needs to be set now
            var hasImplicitExemption = Route.InductionExemptionReason?.RouteImplicitExemption ?? false;

            // initialise a temporary part of journey state for the data collection for a completing route
            if (JourneyInstance!.State.EditStatusState is null)
            {
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
            else
            {
                await JourneyInstance!.UpdateStateAsync(
                    s => s.EditStatusState!.Status = Status);
            }
        }
        else // the status has been changed to something other than 'holds'
        {
            await JourneyInstance!.UpdateStateAsync(s =>
            {
                s.HoldsFrom = null; // clear any previous 'holds' date and exemption
                s.IsExemptFromInduction = null;
                s.Status = Status;
            });
        }

        return Redirect(CompletingRoute ?
            linkGenerator.RoutesToProfessionalStatus.EditRoute.HoldsFrom(QualificationId, JourneyInstance!.InstanceId) :
            FromCheckAnswers ?
                linkGenerator.RoutesToProfessionalStatus.EditRoute.CheckAnswers(QualificationId, JourneyInstance.InstanceId) :
                linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(QualificationId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Qualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        Statuses = ProfessionalStatusStatusRegistry.All.ToArray();
        Route = await referenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId);
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        await next();
    }

    public string BackLink =>
         FromCheckAnswers ?
            linkGenerator.RoutesToProfessionalStatus.EditRoute.CheckAnswers(QualificationId, JourneyInstance!.InstanceId) :
            linkGenerator.RoutesToProfessionalStatus.EditRoute.Detail(QualificationId, JourneyInstance!.InstanceId);

    private bool CompletingRoute => Status is RouteToProfessionalStatusStatus.Holds && (JourneyInstance!.State.CurrentStatus is not RouteToProfessionalStatusStatus.Holds);

    public bool NotCompletedRoute => Status is not RouteToProfessionalStatusStatus.Holds;
}
