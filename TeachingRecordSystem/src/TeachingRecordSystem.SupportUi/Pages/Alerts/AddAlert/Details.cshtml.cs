using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class DetailsModel(SupportUiLinkGenerator linkGenerator, EvidenceUploadManager evidenceUploadManager) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    public string? AlertTypeName { get; set; }

    [BindProperty]
    [MaxLength(UiDefaults.DetailMaxCharacterCount, ErrorMessage = $"Details {UiDefaults.DetailMaxCharacterCountErrorMessage}")]
    public string? Details { get; set; }

    public void OnGet()
    {
        Details = JourneyInstance!.State.Details;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Details = Details;
        });

        return Redirect(FromCheckAnswers
            ? linkGenerator.Alerts.AddAlert.CheckAnswers(PersonId, JourneyInstance.InstanceId)
            : linkGenerator.Alerts.AddAlert.Link(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceUploadManager.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.Persons.PersonDetail.Alerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (JourneyInstance!.State.AlertTypeId is null)
        {
            context.Result = Redirect(linkGenerator.Alerts.AddAlert.Type(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
        AlertTypeName = JourneyInstance.State.AlertTypeName;
    }
}
