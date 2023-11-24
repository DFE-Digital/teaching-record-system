using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class StartDateModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;

    public StartDateModel(
        TrsLinkGenerator linkGenerator)
    {
        _linkGenerator = linkGenerator;
    }

    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Display(Name = "Start date")]
    public DateOnly? StartDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (StartDate is null)
        {
            ModelState.AddModelError(nameof(StartDate), "Enter a start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.StartDate = StartDate);

        return Redirect(_linkGenerator.MqAddResult(PersonId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personDetail = (ContactDetail?)context.HttpContext.Items["CurrentPersonDetail"];

        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        StartDate ??= JourneyInstance!.State.StartDate;

        await next();
    }
}
