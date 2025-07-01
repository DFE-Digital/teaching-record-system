using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Tokens;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class DetailsModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    public string? AlertTypeName { get; set; }

    [BindProperty]
    [Display(Description = "For example, include any restrictions it places on a teacher.")]
    [MaxLength(FileUploadDefaults.DetailMaxCharacterCount, ErrorMessage = $"Details {FileUploadDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? Details { get; set; }

    public void OnGet()
    {
        Details = JourneyInstance!.State.Details;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!Details.IsNullOrEmpty() &&
            JourneyInstance!.State.AlertTypeId == AlertType.ProhibitionBySoSMisconduct ||
            JourneyInstance!.State.AlertTypeId == AlertType.SosDecisionNoProhibition ||
            JourneyInstance!.State.AlertTypeId == AlertType.InterimProhibitionBySoS)
        {
            ModelState.AddModelError(nameof(Details), $"Do not enter details if the alert type is {JourneyInstance!.State.AlertTypeName}");
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
            ? linkGenerator.AlertAddCheckAnswers(PersonId, JourneyInstance.InstanceId)
            : linkGenerator.AlertAddLink(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.AlertTypeId is null)
        {
            context.Result = Redirect(linkGenerator.AlertAddType(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        AlertTypeName = JourneyInstance.State.AlertTypeName;
    }
}
