using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), ActivatesJourney, RequireJourneyInstance]
public class ProviderModel : PageModel
{
    private readonly ReferenceDataCache _referenceDataCache;
    private readonly TrsLinkGenerator _linkGenerator;

    public ProviderModel(
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
    [Display(Name = "Training Provider")]
    public string? MqEstablishmentValue { get; set; }

    public dfeta_mqestablishment[]? MqEstablishments { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(MqEstablishmentValue))
        {
            ModelState.AddModelError(nameof(MqEstablishmentValue), "Select a training provider");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.MqEstablishmentValue = MqEstablishmentValue);

        return Redirect(_linkGenerator.MqAddSpecialism(PersonId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonDetail(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personDetail = (ContactDetail?)context.HttpContext.Items["CurrentPersonDetail"];

        var establishments = await _referenceDataCache.GetMqEstablishments();
        MqEstablishments = establishments
            .OrderBy(e => e.dfeta_name)
            .ToArray();

        PersonName = personDetail!.Contact.ResolveFullName(includeMiddleName: false);
        MqEstablishmentValue ??= JourneyInstance!.State.MqEstablishmentValue;

        await next();
    }
}
