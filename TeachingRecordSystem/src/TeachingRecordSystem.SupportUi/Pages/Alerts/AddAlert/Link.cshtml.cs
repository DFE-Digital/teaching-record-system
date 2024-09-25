using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.AddAlert;

[Journey(JourneyNames.AddAlert), RequireJourneyInstance]
public class LinkModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddAlertState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Enter link")]
    public string? Link { get; set; }

    public void OnGet()
    {
        Link = JourneyInstance!.State.Link;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!string.IsNullOrEmpty(Link) &&
            (!Uri.TryCreate(Link, UriKind.Absolute, out var uri) ||
                (uri.Scheme != "http" && uri.Scheme != "https")))
        {
            ModelState.AddModelError(nameof(Link), "Enter a valid URL");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state =>
        {
            state.Link = Link;
        });

        return Redirect(FromCheckAnswers
            ? linkGenerator.AlertAddCheckAnswers(PersonId, JourneyInstance.InstanceId)
            : linkGenerator.AlertAddStartDate(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonAlerts(PersonId));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (string.IsNullOrEmpty(JourneyInstance!.State.Details))
        {
            context.Result = Redirect(linkGenerator.AlertAddDetails(PersonId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonName = personInfo.Name;
    }
}
