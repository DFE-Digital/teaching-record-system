using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

[Journey(JourneyNames.EditMqSpecialism), RequireJourneyInstance]
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

    public JourneyInstance<EditMqSpecialismState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? CurrentSpecialismName { get; set; }

    public dfeta_specialism? NewSpecialism { get; set; }

    public async Task<IActionResult> OnPost()
    {
        await _crmQueryDispatcher.ExecuteQuery(
            new UpdateMandatoryQualificationSpecialismQuery(
                QualificationId,
                NewSpecialism!.Id));

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
            context.Result = Redirect(_linkGenerator.MqEditSpecialism(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        PersonId = JourneyInstance!.State.PersonId;
        PersonName = JourneyInstance!.State.PersonName;
        CurrentSpecialismName = JourneyInstance!.State.CurrentSpecialismName;
        NewSpecialism = await _referenceDataCache.GetMqSpecialismByValue(JourneyInstance!.State.SpecialismValue!);

        await next();
    }
}
