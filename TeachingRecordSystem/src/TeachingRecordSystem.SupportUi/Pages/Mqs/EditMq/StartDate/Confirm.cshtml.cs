using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.StartDate;

[Journey(JourneyNames.EditMqStartDate), RequireJourneyInstance]
public class ConfirmModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    TrsLinkGenerator linkGenerator,
    IBackgroundJobScheduler backgroundJobScheduler) : PageModel
{
    public JourneyInstance<EditMqStartDateState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public DateOnly? CurrentStartDate { get; set; }

    public DateOnly? NewStartDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        await crmQueryDispatcher.ExecuteQuery(
            new UpdateMandatoryQualificationStartDateQuery(
                QualificationId,
                NewStartDate!.Value));

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification changed");

        await backgroundJobScheduler.Enqueue<TrsDataSyncHelper>(helper => helper.SyncMandatoryQualification(QualificationId, CancellationToken.None));

        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqEditStartDate(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        var personInfo = context.HttpContext.GetCurrentPersonFeature();

        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        CurrentStartDate = JourneyInstance!.State.CurrentStartDate;
        NewStartDate ??= JourneyInstance!.State.StartDate;
    }
}
