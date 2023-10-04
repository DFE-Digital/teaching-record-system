using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using static TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.AlertsModel;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.Alert;

public class IndexModel : PageModel
{
    private readonly TrsLinkGenerator _linkGenerator;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public IndexModel(
        TrsLinkGenerator linkGenerator,
        ICrmQueryDispatcher crmQueryDispatcher)
    {
        _linkGenerator = linkGenerator;
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    [FromRoute]
    public Guid AlertId { get; set; }

    public AlertInfo? Alert { get; set; }

    public bool IsActive { get; set; }

    public Guid? PersonId { get; set; }

    public async Task<IActionResult> OnPost()
    {
        await _crmQueryDispatcher.ExecuteQuery(new UpdateSanctionStateQuery(AlertId, IsActive ? dfeta_sanctionState.Inactive : dfeta_sanctionState.Active));

        IsActive = !IsActive;
        TempData.SetFlashSuccess($"{(IsActive ? "Inactive status removed" : "Status changed to inactive")}");

        return Redirect(_linkGenerator.Alert(AlertId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var sanction = await _crmQueryDispatcher.ExecuteQuery(new GetSanctionDetailsBySanctionIdQuery(AlertId));
        if (sanction is null)
        {
            context.Result = NotFound();
            return;
        }

        Alert = MapSanction(sanction);
        IsActive = sanction.Sanction.StateCode == dfeta_sanctionState.Active;
        PersonId = sanction.Sanction.dfeta_PersonId.Id;

        await next();
    }

    private AlertInfo MapSanction(SanctionDetailResult sanction)
    {
        var alertStatus = sanction.Sanction.StateCode == dfeta_sanctionState.Inactive ? AlertStatus.Inactive :
            sanction.Sanction.dfeta_EndDate is null ? AlertStatus.Active :
            AlertStatus.Closed;

        return new AlertInfo()
        {
            AlertId = sanction.Sanction.Id,
            Description = sanction.Description,
            Details = sanction.Sanction.dfeta_SanctionDetails,
            StartDate = sanction.Sanction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            EndDate = sanction.Sanction.dfeta_EndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            Status = alertStatus
        };
    }
}
