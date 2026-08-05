using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus)]
public class StatusModel(AddRouteJourneyCoordinator journey) : PageModel
{
    private readonly InlineValidator<StatusModel> _validator = new()
    {
        v => v.RuleFor(m => m.Status)
            .NotNull().WithMessage("Select a route status")
    };

    public string PageCaption => journey.PageCaption;

    public string? BackLink { get; set; }

    public RouteToProfessionalStatusType RouteType { get; set; } = null!;

    public ProfessionalStatusStatusInfo[] Statuses { get; set; } = [];

    [BindProperty]
    public bool Cancel { get; set; }

    [BindProperty]
    public RouteToProfessionalStatusStatus? Status { get; set; }

    public void OnGet()
    {
        Status = journey.State.Status;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Cancel)
        {
            return Redirect(await journey.CancelAsync());
        }

        await this.ThrowIfInvalidAsync(_validator);

        return await journey.AnswerAndAdvanceAsync(AddRoutePage.Status, state =>
        {
            state.Status = Status;
            state.IsExemptFromInduction = Status is RouteToProfessionalStatusStatus.Holds
                ? RouteType.InductionExemptionReason?.RouteImplicitExemption
                : null;
        });
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        RouteType = await journey.GetRouteTypeAsync();
        Statuses = ProfessionalStatusStatusRegistry.All.ToArray();

        BackLink = journey.GetBackLink();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
