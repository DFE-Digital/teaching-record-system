using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

[Journey(JourneyNames.EditRouteToProfessionalStatus), RequireJourneyInstance]
public class EndDateModel(
    TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditRouteState>? JourneyInstance { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public string? PersonName { get; set; }

    public Guid PersonId { get; set; }

    [BindProperty]
    [DateInput(ErrorMessagePrefix = "End date")]
    [Required(ErrorMessage = "Enter an end date")]
    [Display(Name = "Enter the route end date")]
    public DateOnly? TrainingEndDate { get; set; }

    public void OnGet()
    {
        TrainingEndDate = JourneyInstance!.State.TrainingEndDate;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (TrainingEndDate.HasValue && JourneyInstance!.State.TrainingStartDate is DateOnly startDate && startDate >= TrainingEndDate)
        {
            ModelState.AddModelError(nameof(TrainingEndDate), "End date must be after start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        if (JourneyInstance!.State.IsCompletingRoute) // if user is here as part of the data collection for awarded or approved state
        {
            await JourneyInstance!.UpdateStateAsync(s => s.EditStatusState!.TrainingEndDate = TrainingEndDate);
        }
        else // user is simply editing the end date
        {
            await JourneyInstance!.UpdateStateAsync(s => s.TrainingEndDate = TrainingEndDate);
        }

        return Redirect(JourneyInstance!.State.IsCompletingRoute ?
            NextCompletingRoutePage() :
            FromCheckAnswers ?
                linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
                linkGenerator.RouteEditDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;
    }

    public string BackLink => FromCheckAnswers ?
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance!.InstanceId) :
            JourneyInstance!.State.IsCompletingRoute ?
                linkGenerator.RouteEditStatus(QualificationId, JourneyInstance!.InstanceId) :
                linkGenerator.RouteEditDetail(QualificationId, JourneyInstance!.InstanceId);

    private string NextCompletingRoutePage()
    {
        return linkGenerator.RouteEditHoldsFrom(QualificationId, JourneyInstance!.InstanceId);
    }
}
