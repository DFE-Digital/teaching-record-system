using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance, CheckProfessionalStatusExistsFilterFactory()]
public class StatusModel(
    TrsLinkGenerator linkGenerator) : PageModel
{
    public string? PersonName { get; set; }
    public Guid PersonId { get; private set; }
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }
    public RouteToProfessionalStatus Route { get; set; } = null!;

    public ProfessionalStatusStatusInfo[] Statuses { get; set; } = [];

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    [BindProperty]
    [Display(Name = "Select the route status")]
    [Required(ErrorMessage = "Select a route status")]
    public ProfessionalStatusStatus Status { get; set; }

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

        await JourneyInstance!.UpdateStateAsync(s => s.Status = Status);

        var justCompletedRoute = JourneyInstance!.State.Status == ProfessionalStatusStatus.Awarded && JourneyInstance!.State.CurrentStatus != ProfessionalStatusStatus.Awarded
            || JourneyInstance!.State.Status == ProfessionalStatusStatus.Approved && JourneyInstance!.State.CurrentStatus != ProfessionalStatusStatus.Approved;

        return Redirect(FromCheckAnswers ?
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            justCompletedRoute ?
                linkGenerator.RouteEditEndDate(QualificationId, JourneyInstance.InstanceId) :
                linkGenerator.RouteDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public IActionResult OnPostCancel()
    {
        return Redirect(linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        Statuses = ProfessionalStatusStatusRegistry.All.ToArray();

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;

        base.OnPageHandlerExecuting(context);
    }
}
