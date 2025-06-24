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

    public string BackLink => PreviousPageUrl;

    public void OnGet()
    {
        Status = JourneyInstance!.State.NewRouteToProfessionalStatusId != null ? JourneyInstance!.State.NewStatus : JourneyInstance!.State.Status;
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
                if (JourneyInstance!.State.NewRouteToProfessionalStatusId == null)
                {
                    x.Begin();
                }

                x.NewStatus = Status;
                x.NewIsExemptFromInduction = Status is RouteToProfessionalStatusStatus.Holds ?
                    Route.InductionExemptionReason?.RouteImplicitExemption
                    : null;
            });

        return await ContinueAsync();
    }

    protected override Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        Statuses = ProfessionalStatusStatusRegistry.All.ToArray();

        return Task.CompletedTask;
    }
}
