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

        await JourneyInstance!.UpdateStateAsync(state => state.TrainingEndDate = TrainingEndDate);
        return Redirect(FromCheckAnswers ?
            linkGenerator.RouteCheckYourAnswers(QualificationId, JourneyInstance.InstanceId) :
            linkGenerator.RouteDetail(QualificationId, JourneyInstance.InstanceId));
    }

    public void OnPostCancel()
    {
        Redirect(linkGenerator.RouteDetail(QualificationId, JourneyInstance!.InstanceId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonName = personInfo.Name;
        PersonId = personInfo.PersonId;
    }
}
