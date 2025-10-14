using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class StatusModel(
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceController)
    : AddRouteCommonPageModel(AddRoutePage.Status, linkGenerator, referenceDataCache, evidenceController)
{
    public override AddRoutePage? NextPage => PageDriver.NextPage(Route, Status!.Value, AddRoutePage.Status) ?? AddRoutePage.CheckAnswers;
    public override AddRoutePage? PreviousPage => AddRoutePage.Route;

    public RouteToProfessionalStatusType Route { get; set; } = null!;

    public ProfessionalStatusStatusInfo[] Statuses { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select a route status")]
    public RouteToProfessionalStatusStatus? Status { get; set; }

    public string PageHeading => "Select the route status";

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
        await base.OnPageHandlerExecutingAsync(context);

        if (JourneyInstance!.State.RouteToProfessionalStatusId is null)
        {
            context.Result = BadRequest();
            return;
        }

        Statuses = ProfessionalStatusStatusRegistry.All.ToArray();
        Route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId.Value);
    }
}
