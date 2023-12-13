using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

[Journey(JourneyNames.DeleteMq), RequireJourneyInstance]
public class ConfirmModel(
    ICrmQueryDispatcher crmQueryDispatcher,
    TrsLinkGenerator linkGenerator) : PageModel
{
    public JourneyInstance<DeleteMqState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid QualificationId { get; set; }

    public Guid? PersonId { get; set; }

    public string? PersonName { get; set; }

    public string? TrainingProvider { get; set; }

    public MandatoryQualificationSpecialism? Specialism { get; set; }

    public MandatoryQualificationStatus? Status { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public MqDeletionReasonOption? DeletionReason { get; set; }

    public string? DeletionReasonDetail { get; set; }

    public async Task<IActionResult> OnPost()
    {
        // Currently adding empty JSON until the deleted event is defined in a future Trello card
        await crmQueryDispatcher.ExecuteQuery(
            new DeleteQualificationQuery(
                QualificationId,
                "{}"));

        await JourneyInstance!.CompleteAsync();
        TempData.SetFlashSuccess("Mandatory qualification deleted");

        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public async Task<IActionResult> OnPostCancel()
    {
        await JourneyInstance!.DeleteAsync();
        return Redirect(linkGenerator.PersonQualifications(PersonId!.Value));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(linkGenerator.MqDelete(QualificationId, JourneyInstance.InstanceId));
            return;
        }

        PersonId = JourneyInstance!.State.PersonId;
        PersonName = JourneyInstance!.State.PersonName;
        TrainingProvider = JourneyInstance!.State.MqEstablishment;
        Specialism = JourneyInstance!.State.Specialism;
        Status = JourneyInstance!.State.Status;
        StartDate = JourneyInstance!.State.StartDate;
        EndDate = JourneyInstance!.State.EndDate;
        DeletionReason = JourneyInstance!.State.DeletionReason;
        DeletionReasonDetail = JourneyInstance?.State.DeletionReasonDetail;

        await next();
    }
}
