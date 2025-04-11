using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class RouteModel(TrsLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

    public string? PersonName { get; set; }

    public RouteToProfessionalStatus[] Routes { get; set; } = [];

    public RouteToProfessionalStatus[] ArchivedRoutes { get; set; } = [];

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Route type")]
    public Guid? RouteId { get; set; }

    [BindProperty]
    [Display(Name = "Inactive route type")]
    public Guid? ArchivedRouteId { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
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
                state.RouteToProfessionalStatusId = RouteId.HasValue ? RouteId.Value : ArchivedRouteId!.Value;
            });
        return Redirect(FromCheckAnswers
            ? linkGenerator.RouteAddCheckAnswers(PersonId, JourneyInstance!.InstanceId)
            : linkGenerator.RouteAddStatus(PersonId, JourneyInstance!.InstanceId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        Routes = await referenceDataCache.GetRoutesToProfessionalStatusAsync(activeOnly: true);
        ArchivedRoutes = await referenceDataCache.GetRoutesToProfessionalStatusArchivedOnlyAsync();
        var preselectedRouteId = JourneyInstance!.State.RouteToProfessionalStatusId;
        if (!Routes.Any(r => r.InductionExemptionReasonId == preselectedRouteId))
        {
            RouteId = preselectedRouteId;
        }
        else
        {
            ArchivedRouteId = preselectedRouteId;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
