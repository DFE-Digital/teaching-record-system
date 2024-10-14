using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts;

[ServiceFilter(typeof(CheckAlertExistsFilter)), ServiceFilter(typeof(RequireClosedAlertFilter))]
public class AlertDetailModel(TrsDbContext dbContext) : PageModel
{
    [FromRoute]
    public Guid AlertId { get; set; }

    public Alert? Alert { get; set; }

    public async Task OnGet()
    {
        Alert = await dbContext.Alerts
            .Include(a => a.AlertType)
            .ThenInclude(at => at.AlertCategory)
            .SingleAsync(a => a.AlertId == AlertId);
    }
}
