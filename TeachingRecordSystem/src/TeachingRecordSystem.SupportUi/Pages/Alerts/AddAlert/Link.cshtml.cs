using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class LinkModel(TrsLinkGenerator linkGenerator, EvidenceUploadManager evidenceController) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Do you want to add a link to a panel outcome?")]
    [Required(ErrorMessage = "Select yes if you want to add a link to a panel outcome")]
    public bool? AddLink { get; set; }

    [BindProperty]
    [Display(Name = "Enter link to panel outcome")]
    public string? Link { get; set; }

    public void OnGet()
    {
        AddLink = JourneyInstance!.State.AddLink;
        Link = JourneyInstance!.State.Link;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (AddLink == true && !TrsUriHelper.TryCreateWebsiteUri(Link, out _))
        {
            ModelState.AddModelError(nameof(Link), "Enter a valid URL");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.AddLink = AddLink;
            state.Link = AddLink == true ? Link : null;
        });

        return Redirect(FromCheckAnswers
            ? linkGenerator.AlertAddCheckAnswers(PersonId, JourneyInstance.InstanceId)
            : linkGenerator.AlertAddStartDate(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancelAsync()
    {
        await evidenceController.DeleteUploadedFileAsync(JourneyInstance!.State.Evidence.UploadedEvidenceFile);
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
    }
}
