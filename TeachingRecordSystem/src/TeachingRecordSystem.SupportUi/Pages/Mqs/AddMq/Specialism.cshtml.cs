using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class SpecialismModel(TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MandatoryQualificationSpecialismInfo[]? Specialisms { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (Specialism is null)
        {
            ModelState.AddModelError(nameof(Specialism), "Select a specialism");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.Specialism = Specialism);

        return Redirect(linkGenerator.MqAddStartDate(PersonId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personDetail = (ContactDetail?)context.HttpContext.Items["CurrentPersonDetail"];

        Specialisms = MandatoryQualificationSpecialismRegistry.All
            .OrderBy(t => t.Title)
            .ToArray();

        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        Specialism ??= JourneyInstance!.State.Specialism;

        await next();
    }
}
