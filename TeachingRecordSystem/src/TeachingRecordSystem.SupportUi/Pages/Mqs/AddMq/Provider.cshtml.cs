using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.AddMq;

[Journey(JourneyNames.AddMq), RequireJourneyInstance]
public class ProviderModel(
    ReferenceDataCache referenceDataCache,
    TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<AddMqState>? JourneyInstance { get; set; }

    [FromQuery]
    public Guid PersonId { get; set; }

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a training provider")]
    [Display(Name = "Training provider")]
    public string? MqEstablishmentValue { get; set; }

    public dfeta_mqestablishment[]? MqEstablishments { get; set; }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.MqEstablishmentValue = MqEstablishmentValue);

        return Redirect(FromCheckAnswers ?
            linkGenerator.MqAddCheckAnswers(PersonId, JourneyInstance.InstanceId) :
            linkGenerator.MqAddSpecialism(PersonId, JourneyInstance.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        var establishments = await referenceDataCache.GetMqEstablishments();
        MqEstablishments = establishments
            .OrderBy(e => e.dfeta_name)
            .ToArray();

        PersonName = personInfo.Name;
        MqEstablishmentValue ??= JourneyInstance!.State.MqEstablishmentValue;

        await next();
    }
}
