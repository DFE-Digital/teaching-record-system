using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

[Journey(JourneyNames.EditMqProvider), ActivatesJourney, RequireJourneyInstance]
public class IndexModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ReferenceDataCache _referenceDataCache;
    private readonly TrsLinkGenerator _linkGenerator;

    public IndexModel(
        ICrmQueryDispatcher crmQueryDispatcher,
        ReferenceDataCache referenceDataCache,
        TrsLinkGenerator linkGenerator)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _referenceDataCache = referenceDataCache;
        _linkGenerator = linkGenerator;
    }

    public JourneyInstance<EditMqProviderState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Select a training provider")]
    [Display(Name = "Training Provider")]
    public string? MqEstablishmentValue { get; set; }

    public dfeta_mqestablishment[]? MqEstablishments { get; set; }

    public void OnGet()
    {
        MqEstablishmentValue ??= JourneyInstance!.State.MqEstablishmentValue;
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        await JourneyInstance!.UpdateStateAsync(state => state.MqEstablishmentValue = MqEstablishmentValue);

        return Redirect(_linkGenerator.MqEditProviderConfirm(QualificationId, JourneyInstance!.InstanceId));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var qualification = await _crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(QualificationId));
        if (qualification is null || qualification.dfeta_Type != dfeta_qualification_dfeta_Type.MandatoryQualification)
        {
            context.Result = NotFound();
            return;
        }

        var establishments = await _referenceDataCache.GetMqEstablishments();
        MqEstablishments = establishments
            .OrderBy(e => e.dfeta_name)
            .ToArray();

        await JourneyInstance!.State.EnsureInitialized(_crmQueryDispatcher, _referenceDataCache, qualification);

        PersonId = JourneyInstance!.State.PersonId;
        PersonName = JourneyInstance!.State.PersonName;

        await next();
    }
}
