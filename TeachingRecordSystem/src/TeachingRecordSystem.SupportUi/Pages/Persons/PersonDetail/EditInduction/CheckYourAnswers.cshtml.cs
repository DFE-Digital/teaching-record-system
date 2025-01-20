using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

[Journey(JourneyNames.EditInduction), RequireJourneyInstance]
public class CheckYourAnswersModel : CommonJourneyPage
{
    private readonly TrsDbContext _dbContext;
    private readonly ReferenceDataCache _referenceDataCache;
    private InductionJourneyPage? StartPage => JourneyInstance!.State.JourneyStartPage;

    protected IClock _clock;

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
                .Join(ExemptionReasons, id => id, reason => reason.InductionExemptionReasonId, (id, reason) => reason.Name)
                .OrderByDescending(name => name);
        }
    }
    public InductionExemptionReason[] ExemptionReasons { get; set; } = Array.Empty<InductionExemptionReason>();
    public InductionChangeReasonOption ChangeReason { get; set; }
    public string? ChangeReasonDetail { get; set; }

    public string? EvidenceFileName { get; set; }

    public string? EvidenceFileSizeDescription { get; set; }

    public string? UploadedEvidenceFileUrl { get; set; }

    public bool ShowStatus =>
            StartPage == InductionJourneyPage.Status;

    public bool ShowStartDate =>
            StartPage != InductionJourneyPage.CompletedDate && InductionStatus.RequiresStartDate();

    public bool ShowCompletedDate => InductionStatus.RequiresCompletedDate();

    public bool ShowExemptionReasons =>
        InductionStatus == InductionStatus.Exempt;

    [FromQuery]
    public bool FromCheckAnswers { get; set; }

    public CheckYourAnswersModel(TrsLinkGenerator linkGenerator, TrsDbContext dbContext, ReferenceDataCache referenceDataCache, IClock clock) : base(linkGenerator)
    {
        _dbContext = dbContext;
        _referenceDataCache = referenceDataCache;
        _clock = clock;
    }

    public string BackLink => PageLink(InductionJourneyPage.ChangeReasons);

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var person = await _dbContext.Persons
            .SingleAsync(q => q.PersonId == PersonId);

        person.SetInductionStatus(
            InductionStatus,
            StartDate,
            CompletedDate,
            JourneyInstance!.State.ExemptionReasonIds ?? Array.Empty<Guid>(),
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
            _clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await _dbContext.AddEventAndBroadcastAsync(updatedEvent);
            await _dbContext.SaveChangesAsync();
        }

        await _dbContext.SaveChangesAsync();

        await JourneyInstance!.CompleteAsync();

        TempData.SetFlashSuccess("Induction details have been updated");

        return Redirect(NextPage()(PersonId, JourneyInstance!.InstanceId));
    }

    public Func<Guid, JourneyInstanceId, string> NextPage()
    {
        return (Id, journeyInstanceId) => LinkGenerator.PersonInduction(Id);
    }

    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        await JourneyInstance!.State.EnsureInitializedAsync(_dbContext, PersonId, InductionJourneyPage.Status);

        ExemptionReasons = await _referenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true);

        var personInfo = context.HttpContext.GetCurrentPersonFeature();
        PersonId = personInfo.PersonId;
        PersonName = personInfo.Name;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        ChangeReason = JourneyInstance!.State.ChangeReason!.Value;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReasonDetail;
        InductionStatus = JourneyInstance!.State.InductionStatus;
        StartDate = JourneyInstance!.State.StartDate;
        CompletedDate = JourneyInstance!.State.CompletedDate;
        await next();
    }
}
