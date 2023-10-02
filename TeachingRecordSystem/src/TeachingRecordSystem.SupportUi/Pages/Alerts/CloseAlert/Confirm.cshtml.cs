using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

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

    [FromRoute]
    public Guid AlertId { get; set; }

    [FromQuery(Name = "endDate")]
    public DateOnly EndDate { get; set; }

    public string? AlertType { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var alert = await _crmQueryDispatcher.ExecuteQuery(new GetSanctionDetailsBySanctionIdQuery(AlertId));
        if (alert is null
            || alert.Sanction.StateCode != Core.Dqt.Models.dfeta_sanctionState.Active
            || alert.Sanction.dfeta_EndDate is not null)
        {
            return NotFound();
        }

        AlertType = alert.Description;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var alert = await _crmQueryDispatcher.ExecuteQuery(new GetSanctionDetailsBySanctionIdQuery(AlertId));
        if (alert is null
            || alert.Sanction.StateCode != Core.Dqt.Models.dfeta_sanctionState.Active
            || alert.Sanction.dfeta_EndDate is not null)
        {
            return NotFound();
        }

        var personId = alert.Sanction.dfeta_PersonId.Id;

        await _crmQueryDispatcher.ExecuteQuery(new CloseSanctionQuery(AlertId, EndDate));

        TempData.SetFlashSuccess("Alert closed");

        return Redirect(_linkGenerator.PersonDetail(personId));
    }
}
