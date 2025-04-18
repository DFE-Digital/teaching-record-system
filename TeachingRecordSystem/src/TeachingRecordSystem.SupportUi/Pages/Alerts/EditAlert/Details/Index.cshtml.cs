using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Details;

[Journey(JourneyNames.EditAlertDetails), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public const int DetailsMaxLength = 4000;

    public JourneyInstance<EditAlertDetailsState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter details")]
    [Display(Description = "For example, include any restrictions it places on a teacher.")]
    [MaxLength(DetailsMaxLength, ErrorMessage = "Details must be 4000 characters or less")]
    public string? Details { get; set; }

    public string? CurrentDetails { get; set; }

    public void OnGet()
    {
        Details = JourneyInstance!.State.Details;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Details == CurrentDetails)
        {
            ModelState.AddModelError(nameof(Details), "Enter changed details");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Details = Details;
        });

        return Redirect(FromCheckAnswers
            ? linkGenerator.AlertEditDetailsCheckAnswers(AlertId, JourneyInstance.InstanceId)
            : linkGenerator.AlertEditDetailsReason(AlertId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var alertInfo = context.HttpContext.GetCurrentAlertFeature();
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        JourneyInstance!.State.EnsureInitialized(alertInfo);

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentDetails = alertInfo.Alert.Details;
    }
}
