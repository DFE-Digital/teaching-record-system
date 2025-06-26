using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class StatusModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(AddRoutePage.Status, linkGenerator, referenceDataCache)
{
    public override AddRoutePage? NextPage => PageDriver.NextPage(Route, Status!.Value, AddRoutePage.Status) ?? AddRoutePage.CheckYourAnswers;
    public override AddRoutePage? PreviousPage => AddRoutePage.Route;

    public RouteToProfessionalStatusType Route { get; set; } = null!;

    public ProfessionalStatusStatusInfo[] Statuses { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select a route status")]
    [Display(Name = "Select the route status")]
    public RouteToProfessionalStatusStatus? Status { get; set; }

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

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Status = Status;
            state.IsExemptFromInduction = Status is RouteToProfessionalStatusStatus.Holds
                ? Route.InductionExemptionReason?.RouteImplicitExemption
                : null;
        });

        return await ContinueAsync();
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.RouteToProfessionalStatusId is null)
        {
            context.Result = BadRequest();
            return;
        }

        Statuses = ProfessionalStatusStatusRegistry.All.ToArray();
        Route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId.Value);

        await base.OnPageHandlerExecutingAsync(context);
    }
}
