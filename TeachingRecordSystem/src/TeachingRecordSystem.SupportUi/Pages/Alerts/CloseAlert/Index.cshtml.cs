using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts.CloseAlert;

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

    [BindProperty(SupportsGet = true)]
    [Display(Name = "End date")]
    public DateOnly? EndDate { get; set; }

    public Guid? PersonId { get; set; }

    public DateOnly? StartDate { get; set; }

    public IActionResult OnPost()
    {
        if (EndDate is null)
        {
            ModelState.AddModelError(nameof(EndDate), "Add an end date");
        }

        if (EndDate <= StartDate)
        {
            ModelState.AddModelError(nameof(EndDate), "End date must be after the start date");
        }

        if (!ModelState.IsValid)
        {
            return this.PageWithErrors();
        }

        return Redirect(_linkGenerator.AlertCloseConfirm(AlertId, EndDate!.Value));
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

        PersonId = alert.Sanction.dfeta_PersonId.Id;
        StartDate = alert.Sanction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true);

        await next();
    }
}
