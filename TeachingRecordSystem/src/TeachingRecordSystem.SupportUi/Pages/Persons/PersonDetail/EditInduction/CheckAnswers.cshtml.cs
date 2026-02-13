using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Shared.Evidence;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class CheckAnswersModel(
    SupportUiLinkGenerator linkGenerator,
    ReferenceDataCache referenceDataCache,
    EvidenceUploadManager evidenceUploadManager,
    IClock clock,
    TrsDbContext dbContext)
    : CommonJourneyPage(dbContext, linkGenerator, evidenceUploadManager)
{
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

    public PersonInductionChangeReason ChangeReason { get; set; }

    public string? ChangeReasonDetail { get; set; }

    public UploadedEvidenceFile? EvidenceFile { get; set; }

    public bool ShowStatusChangeLink => JourneyInstance!.State.JourneyStartPage == InductionJourneyPage.Status;

    public bool ShowStartDateChangeLink =>
        JourneyInstance!.State.JourneyStartPage is InductionJourneyPage.Status or InductionJourneyPage.StartDate && InductionStatus.RequiresStartDate();

    public bool ShowCompletedDateChangeLink => InductionStatus.RequiresCompletedDate();

    public bool ShowExemptionReasonsChangeLink => InductionStatus == InductionStatus.Exempt;

    public string BackLink => GetPageLink(InductionJourneyPage.ChangeReasons);

    protected override async Task OnPageHandlerExecutingAsync(PageHandlerExecutingContext context)
    {
        await base.OnPageHandlerExecutingAsync(context);

        if (!JourneyInstance!.State.IsComplete)
        {
            context.Result = Redirect(GetPageLink(JourneyInstance!.State.JourneyStartPage));
            return;
        }

        ExemptionReasons = await referenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true);
        ChangeReason = JourneyInstance.State.ChangeReason!.Value;
        ChangeReasonDetail = JourneyInstance.State.ChangeReasonDetail;
        InductionStatus = JourneyInstance.State.InductionStatus;
        StartDate = JourneyInstance.State.StartDate;
        CompletedDate = JourneyInstance.State.CompletedDate;
        EvidenceFile = JourneyInstance.State.Evidence.UploadedEvidenceFile;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (JourneyInstance!.State.StartDate > JourneyInstance!.State.CompletedDate)
        {
            return Redirect(
                LinkGenerator.Persons.PersonDetail.EditInduction.CompletedDate(
                    PersonId,
                    JourneyInstance!.InstanceId,
                    fromCheckAnswers: JourneyFromCheckAnswersPage.CheckAnswers));
        }

        var person = await DbContext.Persons.FindAsync(PersonId) ?? throw new InvalidOperationException($"Person with ID {PersonId} not found.");

        person.SetInductionStatus(
            InductionStatus,
            StartDate,
            CompletedDate,
            JourneyInstance!.State.ExemptionReasonIds ?? [],
            ChangeReason.GetDisplayName(),
            ChangeReasonDetail,
            EvidenceFile?.ToEventModel(),
            User.GetUserId(),
            clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await DbContext.AddEventAndBroadcastAsync(updatedEvent);
            await DbContext.SaveChangesAsync();
        }

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Induction details have been updated");

        return Redirect(LinkGenerator.Persons.PersonDetail.Induction(PersonId));
    }
}
