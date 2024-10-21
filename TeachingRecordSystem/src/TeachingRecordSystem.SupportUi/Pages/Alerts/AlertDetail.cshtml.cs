using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts;

[CheckAlertExistsFilterFactory(requiredPermission: Permissions.Alerts.Read), ServiceFilter(typeof(RequireClosedAlertFilter))]
public class AlertDetailModel(IAuthorizationService authorizationService) : PageModel
{
    public Alert? Alert { get; set; }

    public bool CanEdit { get; set; }

    public async Task OnGet()
    {
        Alert = HttpContext.Features.GetRequiredFeature<CurrentAlertFeature>().Alert;

        CanEdit = (await authorizationService.AuthorizeForAlertTypeAsync(
            User,
            Alert.AlertTypeId,
            Permissions.Alerts.Write)) is { Succeeded: true };
    }
}
