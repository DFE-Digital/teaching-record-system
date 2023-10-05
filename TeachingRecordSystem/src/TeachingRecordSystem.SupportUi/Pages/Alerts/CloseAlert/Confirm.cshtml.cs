using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

[Journey(JourneyNames.CloseAlert), RequireJourneyInstance]
public class ConfirmModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public ConfirmModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    public JourneyInstance<CloseAlertState>? JourneyInstance { get; set; }

    [FromRoute]
    public Guid AlertId { get; set; }

    public string? AlertType { get; set; }

    public Guid? PersonId { get; set; }

    public DateOnly EndDate { get; set; }

    public async Task<IActionResult> OnPost()
    {
        await _crmQueryDispatcher.ExecuteQuery(new CloseSanctionQuery(AlertId, EndDate));
        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Alert closed");

        return Redirect(_linkGenerator.PersonAlerts(PersonId!.Value));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var alert = await _crmQueryDispatcher.ExecuteQuery(new GetSanctionDetailsBySanctionIdQuery(AlertId));
        if (alert is null
            || alert.Sanction.StateCode != Core.Dqt.Models.dfeta_sanctionState.Active
            || alert.Sanction.dfeta_EndDate is not null)
        {
            context.Result = NotFound();
            return;
        }

        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(_linkGenerator.AlertClose(AlertId, JourneyInstance!.InstanceId));
            return;
        }

        AlertType = alert.Description;
        PersonId = alert.Sanction.dfeta_PersonId.Id;
        EndDate = JourneyInstance!.State.EndDate.Value;

        await next();
    }
}
