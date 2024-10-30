using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts;

[CheckAlertExistsFilterFactory(requiredPermission: Permissions.Alerts.Read), ServiceFilter(typeof(RequireClosedAlertFilter))]
public class AlertDetailModel(
    IAuthorizationService authorizationService,
    TrsDbContext dbContext) : PageModel
{
    public Alert? Alert { get; set; }

    public string? ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public bool CanEdit { get; set; }

    public async Task OnGet()
    {
        Alert = HttpContext.GetCurrentAlertFeature().Alert;
        var personId = HttpContext.GetCurrentPersonFeature().PersonId;
        var changeReasonInfo = await dbContext.Database
            .SqlQuery<ChangeReasonInfo>(
            $"""
            SELECT
            	payload ->> 'ChangeReason' as change_reason,
            	payload ->> 'ChangeReasonDetail' as change_reason_detail
            FROM
            	events
            WHERE
            	person_id = {personId}
            	AND event_name = 'AlertUpdatedEvent'
            	AND (payload #>> Array['Alert', 'AlertId'])::uuid = {Alert.AlertId}
            	AND payload #>> Array['Alert', 'EndDate'] is not null
            	AND payload #>> Array['OldAlert', 'EndDate'] is null
            ORDER BY
            	created DESC
            """)
            .FirstOrDefaultAsync();

        ChangeReason = changeReasonInfo?.ChangeReason;
        ChangeReasonDetail = changeReasonInfo?.ChangeReasonDetail;
        CanEdit = (await authorizationService.AuthorizeForAlertTypeAsync(
            User,
            Alert.AlertTypeId,
            Permissions.Alerts.Write)) is { Succeeded: true };
    }

    private record ChangeReasonInfo(string ChangeReason, string ChangeReasonDetail);
}
