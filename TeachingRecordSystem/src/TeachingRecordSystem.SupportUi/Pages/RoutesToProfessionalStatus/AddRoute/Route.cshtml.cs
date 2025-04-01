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

    public RouteToProfessionalStatus[] OldRoutes { get; set; } = [];

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [BindProperty]
    [Display(Name = "Route type")]
    [Required(ErrorMessage = "Enter a route type")]
    public Guid? RouteId { get; set; }

    public async Task OnGetAsync()
    {
        Routes = await referenceDataCache.GetRoutesToProfessionalStatusAsync(activeOnly: true);
        OldRoutes = await referenceDataCache.GetRoutesToProfessionalStatusInactiveOnlyAsync();
    }

    public async Task<IActionResult> OnGetCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }
        await JourneyInstance!.UpdateStateAsync(
            state =>
            {
                state.RouteToProfessionalStatusId = RouteId!.Value;
            });
        return Redirect(FromCheckAnswers
            ? linkGenerator.RouteAddCheckAnswers(PersonId, JourneyInstance!.InstanceId)
            : linkGenerator.RouteAddStatus(PersonId, JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;

        base.OnPageHandlerExecuting(context);
    }
}
