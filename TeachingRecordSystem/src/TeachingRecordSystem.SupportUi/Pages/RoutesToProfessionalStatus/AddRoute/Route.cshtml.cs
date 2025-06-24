using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class RouteModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public RouteToProfessionalStatusType[] Routes { get; set; } = [];

    public RouteToProfessionalStatusType[] ArchivedRoutes { get; set; } = [];

    [BindProperty]
    [Display(Name = "Route type")]
    public Guid? RouteId { get; set; }

    [BindProperty]
    [Display(Name = "Inactive route type")]
    public Guid? ArchivedRouteId { get; set; }

    protected override RoutePage CurrentPage => RoutePage.Route;

    public string BackLink => PreviousPageUrl;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (RouteId == null && ArchivedRouteId == null)
        {
            ModelState.AddModelError(nameof(RouteId), "Enter a route type");
        }
        else if (RouteId is not null && ArchivedRouteId is not null)
        {
            ModelState.AddModelError(nameof(RouteId), "Enter only one route type");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(
            state =>
            {
                if (JourneyInstance!.State.NewRouteToProfessionalStatusId == null)
                {
                    state.Begin();
                }

                state.NewRouteToProfessionalStatusId = RouteId.HasValue ? RouteId.Value : ArchivedRouteId!.Value;
            });

        return await ContinueAsync();
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        var allRoutes = await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync();
        Routes = allRoutes.Where(r => r.IsActive).ToArray();
        ArchivedRoutes = allRoutes.Where(r => !r.IsActive).ToArray();

        var preselectedRouteId = JourneyInstance!.State.NewRouteToProfessionalStatusId != null ? JourneyInstance!.State.NewRouteToProfessionalStatusId : JourneyInstance!.State.RouteToProfessionalStatusId;
        if (!Routes.Any(r => r.InductionExemptionReasonId == preselectedRouteId))
        {
            RouteId = preselectedRouteId;
        }
        else
        {
            ArchivedRouteId = preselectedRouteId;
        }
    }
}
