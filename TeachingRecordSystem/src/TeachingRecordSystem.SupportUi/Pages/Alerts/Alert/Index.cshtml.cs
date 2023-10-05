using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.SupportUi.Pages.Common;

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

    public async Task<IActionResult> OnPostSetActive()
    {
        await _crmQueryDispatcher.ExecuteQuery(new UpdateSanctionStateQuery(AlertId, dfeta_sanctionState.Active));

        IsActive = true;
        TempData.SetFlashSuccess("Inactive status removed");

        return Redirect(_linkGenerator.Alert(AlertId));
    }

    public async Task<IActionResult> OnPostSetInactive()
    {
        await _crmQueryDispatcher.ExecuteQuery(new UpdateSanctionStateQuery(AlertId, dfeta_sanctionState.Inactive));

        IsActive = false;
        TempData.SetFlashSuccess("Status changed to inactive");

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
            DetailsLink = sanction.Sanction.dfeta_DetailsLink,
            StartDate = sanction.Sanction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            EndDate = sanction.Sanction.dfeta_EndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            Status = alertStatus
        };
    }
}
