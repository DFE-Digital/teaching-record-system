using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class RouteModel(
    AddRouteJourneyCoordinator journey,
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache) : PageModel
{
    private readonly InlineValidator<RouteModel> _validator = new()
    {
        v => v.RuleFor(m => m.RouteId)
            .Cascade(CascadeMode.Stop)
            .Must((m, routeId) => routeId is not null || m.ArchivedRouteId is not null)
                .WithMessage("Enter a route type")
            .Must((m, routeId) => routeId is null || m.ArchivedRouteId is null)
                .WithMessage("Enter only one route type")
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    public RouteToProfessionalStatusType[] Routes { get; set; } = [];

    public RouteToProfessionalStatusType[] ArchivedRoutes { get; set; } = [];

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public Guid? RouteId { get; set; }

    [BindProperty]
    public Guid? ArchivedRouteId { get; set; }

    public void OnGet()
    {
        var preselectedRouteId = journey.State.RouteToProfessionalStatusId;

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
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        return await journey.AnswerAndAdvanceAsync(
            AddRoutePage.Route,
            state => state.RouteToProfessionalStatusId = RouteId ?? ArchivedRouteId!.Value);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var allRoutes = await referenceDataCache.GetRouteToProfessionalStatusTypesAsync();
        Routes = allRoutes.Where(r => r.IsActive).ToArray();
        ArchivedRoutes = allRoutes.Where(r => !r.IsActive).ToArray();

        BackLink = journey.GetBackLink() ?? linkGenerator.Persons.PersonDetail.Qualifications(journey.PersonId);

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
