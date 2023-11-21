using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class SpecialismModel : PageModel
{
    private readonly ReferenceDataCache _referenceDataCache;
    private readonly TrsLinkGenerator _linkGenerator;

    public SpecialismModel(
        ReferenceDataCache referenceDataCache,
        TrsLinkGenerator linkGenerator)
    {
        _referenceDataCache = referenceDataCache;
        _linkGenerator = linkGenerator;
    }

    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    public string? SpecialismValue { get; set; }

    public dfeta_specialism[]? Specialisms { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (SpecialismValue is null)
        {
            ModelState.AddModelError(nameof(SpecialismValue), "Select a specialism");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.SpecialismValue = SpecialismValue);

        return Redirect(_linkGenerator.MqAddStartDate(PersonId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonDetail(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personDetail = (ContactDetail?)context.HttpContext.Items["CurrentPersonDetail"];

        var specialisms = await _referenceDataCache.GetSpecialisms();
        Specialisms = specialisms
            .OrderBy(e => e.dfeta_name)
            .ToArray();

        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        SpecialismValue ??= JourneyInstance!.State.SpecialismValue;

        await next();
    }
}
