using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class AlertsModel(TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    public Alert[]? OpenAlerts { get; set; }

    public Alert[]? ClosedAlerts { get; set; }

    public async Task OnGet()
    {
        var alerts = await dbContext.Alerts
            .Include(a => a.AlertType)
            .ThenInclude(at => at.AlertCategory)
            .Where(a => a.PersonId == PersonId)
            .ToArrayAsync();

        OpenAlerts = alerts.Where(a => a.IsOpen).OrderBy(a => a.StartDate).ThenBy(a => a.AlertType.Name).ToArray();
        ClosedAlerts = alerts.Where(a => !a.IsOpen).OrderBy(a => a.StartDate).ThenBy(a => a.EndDate).ThenBy(a => a.AlertType.Name).ToArray();
    }
}
