using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class StatusModel(TrsLinkGenerator linkGenerator, ReferenceDataCache referenceDataCache) : PageModel
{
    public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    public RouteToProfessionalStatus Route { get; set; } = null!;

    public ProfessionalStatusStatusInfo[] Statuses { get; set; } = [];

    [BindProperty]
    [Required(ErrorMessage = "Select a route status")]
    [Display(Name = "Select the route status")]
    public ProfessionalStatusStatus? Status { get; set; }

    public string BackLink => FromCheckAnswers ?
        linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance!.InstanceId) :
        linkGenerator.RouteAddRoute(PersonId, JourneyInstance!.InstanceId);

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

        await JourneyInstance!.UpdateStateAsync(x => x.Status = Status);

        var nextPage = PageDriver.NextPage(Route, Status!.Value, AddRoutePage.Status) ?? AddRoutePage.CheckYourAnswers;
        return Redirect(FromCheckAnswers ?
            linkGenerator.RouteAddCheckYourAnswers(PersonId, JourneyInstance.InstanceId) :
            linkGenerator.RouteAddPage(nextPage, PersonId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (JourneyInstance!.State.RouteToProfessionalStatusId is null)
        {
            context.Result = BadRequest();
            return;
        }

        Statuses = ProfessionalStatusStatusRegistry.All.ToArray();
        Route = await referenceDataCache.GetRouteToProfessionalStatusByIdAsync(JourneyInstance!.State.RouteToProfessionalStatusId.Value);
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        await next();
    }
}
