using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
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

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        var state = JourneyInstance!.State;
        if (state.DeactivateReason is null && state.ReactivateReason is null ||
            state.DeactivateReason == DeactivateReasonOption.AnotherReason && state.DeactivateReasonDetail is null ||
            state.ReactivateReason == ReactivateReasonOption.AnotherReason && state.ReactivateReasonDetail is null ||
            state.UploadEvidence is null ||
            state.UploadEvidence == true && state.EvidenceFileId is null)
        {
            context.Result = Redirect(LinkGenerator.PersonSetStatus(PersonId, TargetStatus, JourneyInstance.InstanceId));
            return;
        }

        DeactivateReason = state.DeactivateReason;
        DeactivateReasonDetail = state.DeactivateReasonDetail;
        ReactivateReason = state.ReactivateReason;
        ReactivateReasonDetail = state.ReactivateReasonDetail;
        EvidenceFileId = state.EvidenceFileId;
        EvidenceFileName = state.EvidenceFileName;
        UploadedEvidenceFileUrl = state.EvidenceFileId is not null ?
            await FileService.GetFileUrlAsync(state.EvidenceFileId.Value, FileUploadDefaults.FileUrlExpiry) :
            null;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var now = clock.UtcNow;

        Person!.SetStatus(
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
}
