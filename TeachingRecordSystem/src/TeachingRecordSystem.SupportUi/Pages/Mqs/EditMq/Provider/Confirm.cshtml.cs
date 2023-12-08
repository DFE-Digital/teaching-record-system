using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

[Journey(JourneyNames.EditMqProvider), RequireJourneyInstance]
public class ConfirmModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly ReferenceDataCache _referenceDataCache;
    private readonly TrsLinkGenerator _linkGenerator;

    public ConfirmModel(
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

    public string? CurrentMqEstablishmentName { get; set; }

    public dfeta_mqestablishment? NewMqEstablishment { get; set; }

    public async Task<IActionResult> OnPost()
    {
        await _crmQueryDispatcher.ExecuteQuery(
            new UpdateMandatoryQualificationEstablishmentQuery(
                QualificationId,
                NewMqEstablishment!.Id));

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification changed");

        return Redirect(_linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(_linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(_linkGenerator.MqEditProvider(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        PersonId = JourneyInstance!.State.PersonId;
        PersonName = JourneyInstance!.State.PersonName;
        CurrentMqEstablishmentName = JourneyInstance!.State.CurrentMqEstablishmentName;
        NewMqEstablishment = await _referenceDataCache.GetMqEstablishmentByValue(JourneyInstance!.State.MqEstablishmentValue!);

        await next();
    }
}
