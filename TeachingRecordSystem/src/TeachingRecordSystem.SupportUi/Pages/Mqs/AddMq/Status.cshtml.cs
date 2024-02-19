using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class StatusModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a status")]
    public MandatoryQualificationStatus? Status { get; set; }

    [BindProperty]
    [Display(Name = "End date")]
    public DateOnly? EndDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (Status == MandatoryQualificationStatus.Passed)
        {
            if (EndDate is null)
            {
                ModelState.AddModelError(nameof(EndDate), "Enter an end date");
            }
            else if (EndDate <= StartDate)
            {
                ModelState.AddModelError(nameof(EndDate), "End date must be after start date");
            }
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(
            state =>
            {
                state.Status = Status;
                state.EndDate = Status == MandatoryQualificationStatus.Passed ? EndDate : null;
            });

        return Redirect(FromCheckAnswers ?
            linkGenerator.MqAddCheckAnswers(PersonId, JourneyInstance.InstanceId) :
            linkGenerator.MqAddCheckAnswers(PersonId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        StartDate = JourneyInstance!.State.StartDate;
        Status ??= JourneyInstance.State.Status;
        EndDate ??= JourneyInstance.State.EndDate;
    }
}
