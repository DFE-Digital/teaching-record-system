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
    public string? ExemptionReasonsDisplayString
    {
        get
        {
            if (JourneyInstance?.State.ExemptionReasonIds == null)
            {
                return null;
            }
            var exemptionReasonNames = from id in JourneyInstance.State.ExemptionReasonIds
                                       join reason in ExemptionReasons on id equals reason.InductionExemptionReasonId
                                       orderby reason.Name
                                       select reason.Name;

            return string.Join(", ", exemptionReasonNames);
        }
    }
    public InductionExemptionReason[] ExemptionReasons { get; set; } = Array.Empty<InductionExemptionReason>();
    public InductionChangeReasonOption? ChangeReason { get; set; }
    public string? ChangeReasonDetail { get; set; }
    public string? UploadedEvidenceFileUrl { get; set; }
    public string? EvidenceFileName { get; set; }

    public bool ShowStatus =>
            StartPage == InductionJourneyPage.Status;

    public bool ShowStartDate =>
            StartPage != InductionJourneyPage.CompletedDate &&
                (InductionStatus == InductionStatus.InProgress ||
                InductionStatus == InductionStatus.Passed ||
                InductionStatus == InductionStatus.Failed ||
                InductionStatus == InductionStatus.FailedInWales);
 
    public bool ShowCompletedDate =>
        InductionStatus == InductionStatus.Failed ||
        InductionStatus == InductionStatus.FailedInWales ||
        InductionStatus == InductionStatus.Passed;

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
        // TODO validate the induction state model

        var person = await _dbContext.Persons
            .SingleAsync(q => q.PersonId == PersonId);

        person.SetInductionStatus(
            InductionStatus,
            StartDate,
            CompletedDate,
            JourneyInstance!.State.ExemptionReasonIds ?? Array.Empty<Guid>(),
            User.GetUserId(),
            _clock.UtcNow,
            out var updatedEvent);

        if (updatedEvent is not null)
        {
            await _dbContext.AddEventAndBroadcastAsync(updatedEvent);
            await _dbContext.SaveChangesAsync();
        }

            //ChangeReason,
            //ChangeReasonDetail,
            //JourneyInstance!.State.EvidenceFileName,
            //UploadedEvidenceFileUrl

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
        //EvidenceFileId = JourneyInstance!.State.EvidenceFileId;
        EvidenceFileName = JourneyInstance!.State.EvidenceFileName;
        //UploadedEvidenceFileUrl = JourneyInstance!.State.UploadedEvidenceFileUrl;
        ChangeReason = JourneyInstance!.State.ChangeReason;
        ChangeReasonDetail = JourneyInstance!.State.ChangeReasonDetail;
        InductionStatus = JourneyInstance!.State.InductionStatus;
        StartDate = JourneyInstance!.State.StartDate;
        CompletedDate = JourneyInstance!.State.CompletedDate;

        //EvidenceFileSizeDescription = JourneyInstance!.State.EvidenceFileSizeDescription;
        await next();
    }
}
