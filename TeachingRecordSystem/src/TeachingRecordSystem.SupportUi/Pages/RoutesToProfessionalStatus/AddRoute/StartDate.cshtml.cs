using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

[Journey(JourneyNames.AddRouteToProfessionalStatus), RequireJourneyInstance]
public class StartDateModel(
    TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "Start date")]
    [Required(ErrorMessage = "Enter a start date")]
    [Display(Name = "Enter the route start date")]
    public DateOnly? TrainingStartDate { get; set; }

    public string BackLink => !FromCheckAnswers ?
        linkGenerator.RouteAddStatus(PersonId, JourneyInstance!.InstanceId) :
        linkGenerator.RouteAddCheckAnswers(PersonId, JourneyInstance!.InstanceId);

    public void OnGet()
    {
        TrainingStartDate = JourneyInstance!.State.TrainingStartDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }
        await JourneyInstance!.UpdateStateAsync(state => state.TrainingStartDate = TrainingStartDate);
        if (TrainingStartDate.HasValue && JourneyInstance!.State.TrainingEndDate is DateOnly endDate && TrainingStartDate >= endDate)
        {
            return Redirect(linkGenerator.RouteAddEndDate(PersonId, JourneyInstance.InstanceId, FromCheckAnswers));
        }
        return Redirect(FromCheckAnswers ?
            linkGenerator.RouteAddCheckAnswers(PersonId, JourneyInstance.InstanceId) :
            linkGenerator.RouteAddEndDate(PersonId, JourneyInstance.InstanceId));
    }

    public IActionResult OnPostCancel()
    {
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;
    }
}

