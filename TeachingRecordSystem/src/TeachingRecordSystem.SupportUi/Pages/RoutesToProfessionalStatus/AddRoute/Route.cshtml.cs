using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class RouteModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(AddRoutePage.Route, linkGenerator, referenceDataCache)
{
    public override AddRoutePage? NextPage => AddRoutePage.Status;
    public override AddRoutePage? PreviousPage => null;

    public RouteToProfessionalStatusType[] Routes { get; set; } = [];

    public RouteToProfessionalStatusType[] ArchivedRoutes { get; set; } = [];

    [BindProperty]
    [Display(Name = "Route type")]
    public Guid? RouteId { get; set; }

    [BindProperty]
    [Display(Name = "Inactive route type")]
    public Guid? ArchivedRouteId { get; set; }

    public string PageHeading => "Add route type";

    public void OnGet()
    {
        var preselectedRouteId = JourneyInstance!.State.RouteToProfessionalStatusId;

        if (Routes.Any(r => r.RouteToProfessionalStatusTypeId == preselectedRouteId))
        {
            RouteId = preselectedRouteId;
        }
        else if (ArchivedRoutes.Any(r => r.RouteToProfessionalStatusTypeId == preselectedRouteId))
        {
            ArchivedRouteId = preselectedRouteId;
        }
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

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.RouteToProfessionalStatusId = RouteId ?? ArchivedRouteId!.Value;
        });

        return await ContinueAsync();
    }

    public override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        var allRoutes = await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync();
        Routes = allRoutes.Where(r => r.IsActive).ToArray();
        ArchivedRoutes = allRoutes.Where(r => !r.IsActive).ToArray();
    }
}
