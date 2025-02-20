using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Files;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class CheckYourAnswersModel(
    TrsLinkGenerator linkGenerator,
    TrsDbContext dbContext,
    ReferenceDataCache referenceDataCache,
    IClock clock,
    IFileService fileService)
    : CommonJourneyPage(linkGenerator)
{
    private InductionJourneyPage? StartPage => JourneyInstance!.State.JourneyStartPage;

    public string? PersonName { get; set; }

    public InductionStatus InductionStatus { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? CompletedDate { get; set; }

    public IEnumerable<string>? SelectedExemptionReasonsValues
    {
        get
        {
            if (JourneyInstance?.State.ExemptionReasonIds == null)
            {
                return null;
            }

            return JourneyInstance.State.ExemptionReasonIds
                .Join(ExemptionReasons, id => id, reason => reason.InductionExemptionReasonId, (_, reason) => reason.Name)
                .OrderByDescending(name => name);
        }
    }

    public InductionExemptionReason[] ExemptionReasons { get; set; } = [];

    public InductionChangeReasonOption ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public bool ShowStatusChangeLink => StartPage == InductionJourneyPage.Status;

    public bool ShowStartDateChangeLink =>
        StartPage is InductionJourneyPage.Status or InductionJourneyPage.StartDate && InductionStatus.RequiresStartDate();

    public bool ShowCompletedDateChangeLink => InductionStatus.RequiresCompletedDate();

    public bool ShowExemptionReasonsChangeLink => InductionStatus == InductionStatus.Exempt;

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public string BackLink => GetPageLink(InductionJourneyPage.ChangeReasons);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (JourneyInstance!.State.StartDate > JourneyInstance!.State.CompletedDate)
        {
            return Redirect(
                LinkGenerator.InductionEditCompletedDate(
                    PersonId,
                    JourneyInstance!.InstanceId,
                    fromCheckAnswers: JourneyFromCheckYourAnswersPage.CheckYourAnswers));
        }

        var person = await dbContext.Persons.SingleAsync(q => q.PersonId == PersonId);

        person.SetInductionStatus(
            InductionStatus,
            StartDate,
            CompletedDate,
            JourneyInstance!.State.ExemptionReasonIds ?? [],
            ChangeReason.GetDisplayName(),
            ChangeReasonDetail,
            JourneyInstance!.State.EvidenceFileId is Guid fileId
                ? new EventModels.File()
                {
                    FileId = fileId,
                    Name = JourneyInstance.State.EvidenceFileName!
                }
                : null,
            User.GetUserId(),
            clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await dbContext.AddEventAndBroadcastAsync(updatedEvent);
        }

        await dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Induction details have been updated");

        return Redirect(LinkGenerator.PersonInduction(PersonId));
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(GetPageLink(JourneyInstance!.State.JourneyStartPage));
            return;
        }

        await JourneyInstance!.State.EnsureInitializedAsync(dbContext, PersonId, InductionJourneyPage.Status);

        ExemptionReasons = await referenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        ChangeReason = JourneyInstance!.State.ChangeReason!.Value;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReasonDetail;
        InductionStatus = JourneyInstance!.State.InductionStatus;
        StartDate = JourneyInstance!.State.StartDate;
        CompletedDate = JourneyInstance!.State.CompletedDate;
        UploadedEvidenceFileUrl = JourneyInstance!.State.EvidenceFileId is not null ?
            await fileService.GetFileUrlAsync(JourneyInstance!.State.EvidenceFileId!.Value, FileUploadDefaults.FileUrlExpiry) :
            null;

        await next();
    }
}
