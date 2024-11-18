using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.EditAlert.Link;

[Journey(JourneyNames.EditAlertLink), ActivatesJourney, RequireJourneyInstance]
public class IndexModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<EditAlertLinkState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public bool? AddLink { get; set; }

    [BindProperty]
    [Display(Name = "Enter link to panel outcome")]
    public string? Link { get; set; }

    public string? CurrentLink { get; set; }

    public void OnGet()
    {
        AddLink = JourneyInstance!.State.AddLink;
        Link = JourneyInstance!.State.Link;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (AddLink is null)
        {
            if (string.IsNullOrEmpty(CurrentLink))
            {
                ModelState.AddModelError(nameof(AddLink), "Select yes if you want to add a link to a panel outcome");
            }
            else
            {
                ModelState.AddModelError(nameof(AddLink), "Select change link if you want to change the link to the panel outcome");
            }
        }
        else if (AddLink == true)
        {
            if (!TrsUriHelper.TryCreateWebsiteUri(Link, out _))
            {
                ModelState.AddModelError(nameof(Link), "Enter a valid URL");
            }
            else if (Link == CurrentLink)
            {
                ModelState.AddModelError(nameof(Link), "Enter a different link");
            }
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

        if (string.IsNullOrEmpty(CurrentLink) && AddLink == false)
        {
            return await OnPostCancelAsync();
        }

        return Redirect(FromCheckAnswers
            ? linkGenerator.AlertEditLinkCheckAnswers(AlertId, JourneyInstance.InstanceId)
            : linkGenerator.AlertEditLinkReason(AlertId, JourneyInstance.InstanceId));
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
        CurrentLink = alertInfo.Alert.ExternalLink;
    }
}
