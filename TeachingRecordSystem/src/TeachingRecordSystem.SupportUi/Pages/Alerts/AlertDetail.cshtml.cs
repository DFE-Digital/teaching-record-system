using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;
using TeachingRecordSystem.SupportUi.Infrastructure.Security;

namespace TeachingRecordSystem.SupportUi.Pages.Alerts;

[CheckAlertExistsFilterFactory(requiredPermission: Permissions.Alerts.Read), ServiceFilter(typeof(RequireClosedAlertFilter))]
public class AlertDetailModel(
    IAuthorizationService authorizationService,
    TrsDbContext dbContext,
    IFileService fileService) : PageModel
{
    public Alert? Alert { get; set; }

    public Uri? ExternalLinkUri { get; set; }

    public string? ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

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
            	payload ->> 'ChangeReasonDetail' as change_reason_detail,
                (payload #>> Array['EvidenceFile', 'FileId'])::uuid as evidence_file_id,
                payload #>> Array['EvidenceFile', 'Name'] as evidence_file_name
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
        EvidenceFileName = changeReasonInfo?.EvidenceFileName;
        UploadedEvidenceFileUrl = changeReasonInfo?.EvidenceFileId is not null ?
            await fileService.GetFileUrl(changeReasonInfo.EvidenceFileId!.Value, AlertDefaults.FileUrlExpiry) :
            null;
        ExternalLinkUri = TrsUriHelper.TryCreateWebsiteUri(Alert.ExternalLink, out var linkUri) ? linkUri : null;

        CanEdit = (await authorizationService.AuthorizeForAlertTypeAsync(
            User,
            Alert.AlertTypeId,
            Permissions.Alerts.Write)) is { Succeeded: true };
    }

    private record ChangeReasonInfo(string? ChangeReason, string? ChangeReasonDetail, Guid? EvidenceFileId, string? EvidenceFileName);
}
