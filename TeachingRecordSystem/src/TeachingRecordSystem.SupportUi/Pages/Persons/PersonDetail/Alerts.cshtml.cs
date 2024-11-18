using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;

public class AlertsModel(TrsDbContext dbContext, ReferenceDataCache referenceDataCache, IAuthorizationService authorizationService) : PageModel
{
    [FromRoute]
    public Guid PersonId { get; set; }

    public AlertWithPermissions[]? OpenAlerts { get; set; }

    public AlertWithPermissions[]? ClosedAlerts { get; set; }

    public bool CanAddAlert { get; set; }

    public bool ShowOpenAlertFlag { get; set; }

    public async Task OnGetAsync()
    {
        var alerts = await dbContext.Alerts
            .Include(a => a.AlertType)
            .ThenInclude(at => at.AlertCategory)
            .Where(a => a.PersonId == PersonId)
            .ToArrayAsync();

        var alertTypePermissions = await alerts
            .Select(a => a.AlertTypeId)
            .Distinct()
            .ToAsyncEnumerable()
            .SelectAwait(async id => (
                AlertTypeId: id,
                CanFlag: await authorizationService.AuthorizeForAlertTypeAsync(User, id, Permissions.Alerts.Flag) is { Succeeded: true },
                CanRead: await authorizationService.AuthorizeForAlertTypeAsync(User, id, Permissions.Alerts.Read) is { Succeeded: true },
                CanWrite: await authorizationService.AuthorizeForAlertTypeAsync(User, id, Permissions.Alerts.Write) is { Succeeded: true }))
            .ToDictionaryAsync(t => t.AlertTypeId);

        var authorizedAlerts = alerts
            .Where(a => alertTypePermissions[a.AlertTypeId].CanRead)
            .Select(a =>
                {
                    TrsUriHelper.TryCreateWebsiteUri(a.ExternalLink, out var externalLinkUri);
                    return new AlertWithPermissions(a, externalLinkUri, alertTypePermissions[a.AlertTypeId].CanWrite);
                });

        OpenAlerts = authorizedAlerts.Where(a => a.Alert.IsOpen).OrderBy(a => a.Alert.StartDate).ThenBy(a => a.Alert.AlertType.Name).ToArray();
        ClosedAlerts = authorizedAlerts.Where(a => !a.Alert.IsOpen).OrderBy(a => a.Alert.StartDate).ThenBy(a => a.Alert.EndDate).ThenBy(a => a.Alert.AlertType.Name).ToArray();

        CanAddAlert = await referenceDataCache.GetAlertTypesAsync(activeOnly: true)
            .ToAsyncEnumerableAsync()
            .AnyAwaitAsync(async at => (await authorizationService.AuthorizeForAlertTypeAsync(User, at.AlertTypeId, Permissions.Alerts.Write)) is { Succeeded: true });

        ShowOpenAlertFlag = alerts.Any(a => a.IsOpen && alertTypePermissions[a.AlertTypeId] is { CanFlag: true, CanRead: false });
    }

    public record AlertWithPermissions(Alert Alert, Uri? ExternalLinkUri, bool CanWrite);
}
