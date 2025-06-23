using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class StatusModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache)
    : AddRouteCommonPageModel(linkGenerator, referenceDataCache)
{
    public ProfessionalStatusStatusInfo[] Statuses { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select a route status")]
    [Display(Name = "Select the route status")]
    public new RouteToProfessionalStatusStatus? Status { get; set; }

    protected override RoutePage CurrentPage => RoutePage.Status;

    public string BackLink => PreviousPage;

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

        await JourneyInstance!.UpdateStateAsync(
            x =>
            {
                x.Status = Status;
                x.IsExemptFromInduction = Status is RouteToProfessionalStatusStatus.Holds ?
                    Route.InductionExemptionReason?.RouteImplicitExemption
                    : null;
            });

        return Redirect(NextPage);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        Statuses = ProfessionalStatusStatusRegistry.All.ToArray();

        await base.OnPageHandlerExecutionAsync(context, next);
    }
}
