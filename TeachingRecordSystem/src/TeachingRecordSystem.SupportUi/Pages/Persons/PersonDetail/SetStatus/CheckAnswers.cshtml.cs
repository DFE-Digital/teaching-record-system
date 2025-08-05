using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;
using TeachingRecordSystem.SupportUi.Infrastructure.Filters;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;

[RequireFeatureEnabledFilterFactory(FeatureNames.ContactsMigrated)]
[Journey(JourneyNames.SetStatus), RequireJourneyInstance]
[AllowDeactivatedPerson]
public class CheckAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    IClock clock,
    IFileService fileService)
    : CommonJourneyPage(dbContext, linkGenerator, fileService)
{
    private Person? _person;

    public DeactivateReasonOption? DeactivateReason { get; set; }
    public string? DeactivateReasonDetail { get; set; }
    public ReactivateReasonOption? ReactivateReason { get; set; }
    public string? ReactivateReasonDetail { get; set; }
    public Guid? EvidenceFileId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceFileSizeDescription { get; set; }
    public string? UploadedEvidenceFileUrl { get; set; }

    public string BackLink => LinkGenerator.PersonSetStatus(PersonId, TargetStatus, JourneyInstance!.InstanceId);
    public string ChangeReasonLink => LinkGenerator.PersonSetStatus(PersonId, TargetStatus, JourneyInstance!.InstanceId, fromCheckAnswers: true);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var now = clock.UtcNow;

        _person!.SetStatus(
            TargetStatus,
            TargetStatus == PersonStatus.Deactivated
                ? DeactivateReason!.GetDisplayName()
                : ReactivateReason!.GetDisplayName(),
            TargetStatus == PersonStatus.Deactivated
                ? DeactivateReasonDetail
                : ReactivateReasonDetail,
            EvidenceFileId is Guid fileId
                    ? new EventModels.File()
                    {
                        FileId = fileId,
                        Name = EvidenceFileName!
                    }
                    : null,
            User.GetUserId(),
            now,
            out var @event);

        if (@event is not null)
        {
            await DbContext.AddEventAndBroadcastAsync(@event);
            await DbContext.SaveChangesAsync();
        }

        await JourneyInstance!.CompleteAsync();

        var action = TargetStatus == PersonStatus.Deactivated ? "deactivated" : "reactivated";
        TempData.SetFlashSuccess(messageText: $"{PersonName}\u2019s record has been {action}.");

        return Redirect(LinkGenerator.PersonDetail(PersonId));
    }

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        _person = await DbContext.Persons
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(u => u.PersonId == PersonId);

        if (_person is null)
        {
            context.Result = NotFound();
            return;
        }

        DeactivateReason = JourneyInstance!.State.DeactivateReason;
        DeactivateReasonDetail = JourneyInstance.State.DeactivateReasonDetail;
        ReactivateReason = JourneyInstance.State.ReactivateReason;
        ReactivateReasonDetail = JourneyInstance.State.ReactivateReasonDetail;
        EvidenceFileId = JourneyInstance.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance.State.EvidenceFileName;
        UploadedEvidenceFileUrl = JourneyInstance.State.EvidenceFileId is not null ?
            await FileService.GetFileUrlAsync(JourneyInstance.State.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }
}
