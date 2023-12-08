using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Result;

[Journey(JourneyNames.EditMqResult), RequireJourneyInstance]
public class ConfirmModel : PageModel
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly TrsLinkGenerator _linkGenerator;

    public ConfirmModel(
        ICrmQueryDispatcher crmQueryDispatcher,
        TrsLinkGenerator linkGenerator)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
        _linkGenerator = linkGenerator;
    }

    public JourneyInstance<EditMqResultState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public dfeta_qualification_dfeta_MQ_Status? CurrentResult { get; set; }

    public dfeta_qualification_dfeta_MQ_Status? NewResult { get; set; }

    public DateOnly? CurrentEndDate { get; set; }

    public DateOnly? NewEndDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        await _crmQueryDispatcher.ExecuteQuery(
            new UpdateMandatoryQualificationStatusQuery(
                QualificationId,
                NewResult!.Value,
                NewEndDate));

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
            context.Result = Redirect(_linkGenerator.MqEditResult(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        PersonId = JourneyInstance!.State.PersonId;
        PersonName = JourneyInstance!.State.PersonName;
        CurrentResult = JourneyInstance!.State.CurrentResult;
        NewResult ??= JourneyInstance!.State.Result;
        CurrentEndDate = JourneyInstance!.State.CurrentEndDate;
        NewEndDate ??= JourneyInstance!.State.EndDate;

        await next();
    }
}
